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

namespace Kostassoid.Nerve.EventStore.Model
{
	using System;
	using System.Collections.Generic;
	using System.Threading;

	public abstract class AggregateRoot : IAggregateRoot
	{
		private IList<IDomainEvent> _uncommited = new List<IDomainEvent>();

		public Guid Id { get; protected set; }
		public long Version { get; protected set; }

		protected AggregateRoot()
		{
			Version = 0;
		}

		protected AggregateRoot(Guid id)
		{
			Id = id;
		}

		public void Apply(IDomainEvent ev, bool isReplaying = false)
		{
			DomainSettings.Apply(this, ev);

			Version++;

			if (!isReplaying)
			{
				_uncommited.Add(ev);
			}
		}

		public UncommitedEventStream Flush()
		{
			var toCommit = Interlocked.Exchange(ref _uncommited, new List<IDomainEvent>());
			return new UncommitedEventStream(this, toCommit);
		}
	}
}