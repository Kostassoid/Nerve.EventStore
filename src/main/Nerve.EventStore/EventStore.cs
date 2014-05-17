// Copyright 2014 https://github.com/Kostassoid/Nerve.EventStore
//   
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use 
// this file except in compliance with the License. You may obtain a copy of the 
// License at 
//  
//      http://www.apache.org/licenses/LICENSE-2.0 
//  
// Unless required by applicable law or agreed to in writing, software distributed 
// under the License is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR 
// CONDITIONS OF ANY KIND, either express or implied. See the License for the 
// specific language governing permissions and limitations under the License.

namespace Kostassoid.Nerve.EventStore
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Threading.Tasks;
	using Core;
	using Core.Processing.Operators;
	using Core.Scheduling;
	using Core.Tpl;
	using Model;
	using Storage;
	using Tools;

	public class EventStore : Cell, IEventStore
	{
		readonly IEventStorage _storage;

		public EventStore(IEventStorage storage) : base("EventStore", ThreadScheduler.Factory)
		{
			_storage = storage;

			OnStream().Of<UncommitedEventStream>().ReactWith(ProcessUncommited);
		}

		void ProcessUncommited(ISignal<UncommitedEventStream> uncommited)
		{
			var root = uncommited.Payload.Root;
			var last = _storage.LoadLast(root.Id);
			var toCommit = uncommited.Payload.UncommitedEvents;
			var commited = new List<IDomainEvent>();

			var targetVersion = last != null ? last.TargetVersion + last.Events.Count : 0;
			var currentVersion = targetVersion;
			foreach (var ev in toCommit)
			{
				if (currentVersion != ev.Version)
				{
					throw new ConcurrencyException(currentVersion, ev);
				}

				commited.Add(ev);
				currentVersion++;
			}

			if (commited.Any())
			{
				var commit = new Commit(
					last != null ? last.Id + 1 : 0,
					root.Id,
					targetVersion,
					commited
					);

				_storage.Store(commit);
			}

			uncommited.Return(uncommited.Payload.UncommitedEvents);
		}

		private IAggregateRoot InternalLoad(Type type, Guid id)
		{
			var last = _storage.LoadLast(id);
			if (last == null)
			{
				throw new InvalidOperationException(string.Format("Aggregate root of type {0} with id {1} not found.", type.Name, id));
			}

			var root = (IAggregateRoot)TypeHelpers.New(type);

			foreach (var commit in _storage.LoadSince(id, 0))
			{
				foreach (var ev in commit.Events)
				{
					root.Apply(ev, true);
				}
			}

			return root;
		}

		public Task Commit(IAggregateRoot root)
		{
			return this.SendFor<object>(root.Flush());
		}

		public Task<T> Load<T>(Guid id) where T : IAggregateRoot
		{
			var completion = new TaskCompletionSource<T>();
			completion.SetResult((T)InternalLoad(typeof (T), id));
			return completion.Task;
		}

		public Task OnLoaded<T>(Guid id, Action<T> action)
		{
			var root = InternalLoad(typeof (T), id);
			action((T) root);
			return Commit(root);
		}
	}
}