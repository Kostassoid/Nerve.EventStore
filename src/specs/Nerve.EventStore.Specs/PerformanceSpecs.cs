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
	using System.Diagnostics;
	using System.Linq;
	using System.Threading.Tasks;
	using Command;
	using Core.Tpl;
	using Machine.Specifications;
	using Model;

	// ReSharper disable InconsistentNaming
	// ReSharper disable UnusedMember.Local
	public class PerformanceSpecs
	{
		[Subject(typeof(EventStore), "Performance")]
		[Tags("Unit")]
		public class when_heavy_working_with_several_aggregates
		{
			const int TotalOps = 30000;

			static EventStore _store;

			static double _ops;

			static Guid[] _ids;

			private Cleanup after = () => _store.Dispose();

			private Establish context = () =>
			{
				_store = new EventStore(new InMemoryEventStorage());
				_ids = Enumerable.Range(1, 10).Select(_ => Guid.NewGuid()).ToArray();

				foreach (var id in _ids)
				{
					var user = User.Create(id, "Joe", 0);
					_store.Commit(user).Wait();
				}
			};

			private Because of = () =>
			{

				var r = new Random();

				var stopwatch = Stopwatch.StartNew();

				var tasks = Enumerable.Range(0, TotalOps)
					.Select(_ => Task.Factory.StartNew(() =>
					{
						var id = _ids[r.Next(10)];

						var user = _store.Load<User>(id).Result;
						user.Birthday();
						_store.Commit(user).Wait();
						//_store.OnLoaded<User>(id, u => u.Birthday()).Wait();
					})).ToArray();

				try
				{
					Task.WaitAll(tasks);
				}
				// ReSharper disable once EmptyGeneralCatchClause
				catch
				{
					// handle
				}

				_ops = TotalOps / stopwatch.Elapsed.TotalSeconds;

				Console.WriteLine("Ops/s: {0}", _ops);
			};

			private It should_be_fast = () => _ops.ShouldBeGreaterThan(5000);

			private It should_be_consistent = () =>
			{
				var total = _ids.Sum(id => _store.Load<User>(id).Result.Age);
				total.ShouldEqual(TotalOps);
			};
		}

		[Subject(typeof(EventStore), "Performance")]
		[Tags("Unit")]
		public class when_handling_commands_concurrently
		{
			const int TotalOps = 30000;

			static EventStore _store;
			static CommandHandler2 _commandHandler;

			static double _ops;

			static Guid[] _ids;

			private Cleanup after = () => _store.Dispose();

			private Establish context = () =>
			{
				_store = new EventStore(new InMemoryEventStorage());
				_commandHandler = new CommandHandler2(_store);
				_ids = Enumerable.Range(1, 10).Select(_ => Guid.NewGuid()).ToArray();

				foreach (var id in _ids)
				{
					var user = User.Create(id, "Joe", 0);
					_store.Commit(user).Wait();
				}
			};

			private Because of = () =>
			{
				var r = new Random();

				var stopwatch = Stopwatch.StartNew();

				var tasks = Enumerable.Range(0, TotalOps)
					.Select(_ => Task.Factory.StartNew(() =>
					{
						var id = _ids[r.Next(10)];

						_commandHandler.Handle(new CelebrateUserBirthday(id));
					})).ToArray();

				try
				{
					Task.WaitAll(tasks);
				}
				// ReSharper disable once EmptyGeneralCatchClause
				catch
				{
					// handle
				}

				_ops = TotalOps / stopwatch.Elapsed.TotalSeconds;

				Console.WriteLine("Ops/s: {0}", _ops);
			};

			private It should_be_fast = () => _ops.ShouldBeGreaterThan(5000);

			private It should_be_consistent = () =>
			{
				var total = _ids.Sum(id => _store.Load<User>(id).Result.Age);
				total.ShouldEqual(TotalOps);
			};
		}


	}

	// ReSharper restore InconsistentNaming
	// ReSharper restore UnusedMember.Local
}