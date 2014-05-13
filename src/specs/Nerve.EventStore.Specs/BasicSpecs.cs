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

			private static Guid _id;
			private static User _loaded;

			private Cleanup after = () =>
			{
				_store.Dispose();
				EventBus.Reset();
			};

			private Establish context = () =>
			{
				_store = new EventStore(new InMemoryEventStorage());
				EventBus.OnStream().Of<UncommitedEventStream>().ReactWith(_store);

				_id = Guid.NewGuid();
				var user = User.Create(_id, "Joe", 33);
				user.Commit().Wait();
			};

			private Because of = () =>
			{
				_loaded = _store.Load<User>(_id);
			};

			private It should_not_be_null = () => _loaded.ShouldNotBeNull();

			private It should_have_properties_set = () =>
			{
				_loaded.Id.ShouldEqual(_id);
				_loaded.Name.ShouldEqual("Joe");
				_loaded.Age.ShouldEqual(33);
			};
		}

		[Subject(typeof(EventStore), "Basic")]
		[Tags("Unit")]
		public class when_loading_updated_aggregate
		{
			private static EventStore _store;

			private static Guid _id;
			private static User _loaded;

			private Cleanup after = () =>
			{
				_store.Dispose();
				EventBus.Reset();
			};

			private Establish context = () =>
			{
				_store = new EventStore(new InMemoryEventStorage());
				EventBus.OnStream().Of<UncommitedEventStream>().ReactWith(_store);

				_id = Guid.NewGuid();
				var user = User.Create(_id, "Joe", 33);
				user.Commit().Wait();

				user = _store.Load<User>(_id);
				user.ChangeName("Bill");
				user.Birthday();
				user.Commit().Wait();
			};

			private Because of = () =>
			{
				_loaded = _store.Load<User>(_id);
			};

			private It should_not_be_null = () => _loaded.ShouldNotBeNull();

			private It should_have_updated_properties = () =>
			{
				_loaded.Id.ShouldEqual(_id);
				_loaded.Name.ShouldEqual("Bill");
				_loaded.Age.ShouldEqual(34);
			};
		}

	}

	// ReSharper restore InconsistentNaming
	// ReSharper restore UnusedMember.Local
}