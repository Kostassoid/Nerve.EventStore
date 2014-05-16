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
			OnStream().Of<AggregateIdentity>().ReactWith(Load);
		}

		void Load(ISignal<AggregateIdentity> signal)
		{
			signal.Return(InternalLoad(signal.Payload.Type, signal.Payload.Id));
		}

		void ProcessUncommited(ISignal<UncommitedEventStream> uncommited)
		{
			var root = uncommited.Payload.Root;
			var loaded = _storage.Load(root.GetType(), root.Id).ToList();
			var sorted = uncommited.Payload.UncommitedEvents.OrderBy(e => e.Version);
			var commited = new List<IDomainEvent>();

			var currentVersion = loaded.Any() ? loaded.Last().Version + 1 : 0;
			foreach (var ev in sorted)
			{
				if (currentVersion != ev.Version)
				{
					throw new ConcurrencyException(
						string.Format("Expected version of {0} ({1}) to be {2} but got {3}."
							, ev.Type, ev.Id, currentVersion, ev.Version));
				}

				loaded.Add(ev);
				commited.Add(ev);
				currentVersion++;
			}

			if (loaded.Any())
			{
				_storage.Save(root.GetType(), root.Id, loaded);
			}

			uncommited.Return(uncommited.Payload.UncommitedEvents);
		}

		private IAggregateRoot InternalLoad(Type type, Guid id)
		{
			var loaded = _storage.Load(type, id).ToList();
			if (!loaded.Any())
			{
				throw new InvalidOperationException(string.Format("Aggregate root of type {0} with id {1} not found.", type.Name, id));
			}

			var root = (IAggregateRoot)TypeHelpers.New(type);
			foreach (var ev in loaded)
			{
				root.Apply(ev, true);
			}

			return root;
		}

		public Task Commit(IAggregateRoot root)
		{
			return this.SendFor<object>(root.Flush());
		}

		public Task<T> Load<T>(Guid id) where T : IAggregateRoot
		{
			return this.SendFor<T>(new AggregateIdentity(typeof(T), id));
		}
	}
}