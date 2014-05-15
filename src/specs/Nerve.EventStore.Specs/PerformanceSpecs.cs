﻿// Copyright 2014 https://github.com/Kostassoid/Nerve.EventStore
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
			const int TotalOps = 10000;

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
						var user = _store.Load<User>(_ids[r.Next(10)]).Result;
						user.Birthday();
						_store.Commit(user).Wait();
					})).ToArray();

				try
				{
					Task.WaitAll(tasks);
				}
				catch
				{
					// handle
				}

				_ops = TotalOps/stopwatch.Elapsed.TotalSeconds;

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