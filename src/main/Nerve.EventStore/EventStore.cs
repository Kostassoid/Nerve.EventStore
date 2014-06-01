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

	public class EventStore : IEventStore, IDisposable
	{
		const int ProcessorsCount = 12;

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

		public Task Commit(IAggregateRoot root)
		{
			var p = Math.Abs(root.Id.GetHashCode() % ProcessorsCount);
			return _processors[p].SendFor<object>(root.Flush());
		}

		public Task<T> Load<T>(Guid id) where T : IAggregateRoot
		{
			var p = Math.Abs(id.GetHashCode() % ProcessorsCount);
			return _processors[p].SendFor<T>(new AggregateIdentity(typeof(T), id));
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