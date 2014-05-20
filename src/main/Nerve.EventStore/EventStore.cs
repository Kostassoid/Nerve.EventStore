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
	using Core.Tpl;
	using Model;
	using Storage;
	using Tools;

	public class EventStore : IEventStore, IDisposable
	{
		const int ProcessorsCount = 4;

		readonly IEventStorage _storage;
		readonly IList<EventStreamProcessor> _processors; 

		public EventStore(IEventStorage storage)
		{
			_storage = storage;

			_processors = Enumerable
				.Range(0, ProcessorsCount)
				.Select(_ => new EventStreamProcessor(_storage))
				.ToList();
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
			var p = Math.Abs(root.Id.GetHashCode() % ProcessorsCount);
			return _processors[p].SendFor<object>(root.Flush());
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

		public void Dispose()
		{
			foreach (var processor in _processors)
			{
				processor.Dispose();
			}
		}
	}
}