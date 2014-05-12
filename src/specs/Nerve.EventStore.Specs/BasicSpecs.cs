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

namespace Kostassoid.Nerve.EventStore.Specs
{
	using System;
	using Core;
	using Core.Processing.Operators;
	using Machine.Specifications;
	using Model;
	using Nerve.EventStore.Model;

	// ReSharper disable InconsistentNaming
	// ReSharper disable UnusedMember.Local
	public class BasicSpecs
	{
		[Subject(typeof(EventStore), "Basic")]
		[Tags("Unit")]
		public class when_loading_new_stored_aggregate
		{
			private static EventStore _store;

			private static User _stored;
			private static User _loaded;

			private Cleanup after = () => _store.Dispose();

			private Establish context = () =>
			{
				_store = new EventStore(new InMemoryEventStorage());
				EventBus.OnStream().Of<UncommitedEventStream>().ReactWith(_store);
			};

			private Because of = () =>
			{
				_stored = User.Create(Guid.NewGuid(), "Joe", 33);
				_stored.Commit();

				_loaded = _store.Load<User>(_stored.Id);
			};

			private It should_not_be_null = () => _loaded.ShouldNotBeNull();

			private It should_be_in_valid_state = () =>
			{
				_loaded.Id.ShouldEqual(_stored.Id);
				_loaded.Name.ShouldEqual(_stored.Name);
				_loaded.Age.ShouldEqual(_stored.Age);
			};
		}

	}

	// ReSharper restore InconsistentNaming
	// ReSharper restore UnusedMember.Local
}