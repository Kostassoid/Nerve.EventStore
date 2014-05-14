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

namespace Kostassoid.Nerve.EventStore
{
	using System;
	using System.Threading;
	using System.Threading.Tasks;
	using Core.Processing.Operators;
	using Model;

	public class EventStore
	{
		EventStoreProcessor _processor;
		Action _unsubscribeAction = () => {};

		public void Configure(Action<IEventStoreConfigurator> configAction)
		{
			var configuration = new EventStoreConfiguration();
			configAction(configuration);

			_unsubscribeAction();

			var processor = new EventStoreProcessor(configuration.Storage);
			var unsubscribe = configuration.Source.OnStream().Of<UncommitedEventStream>().ReactWith(DomainBus.Cell);
			_unsubscribeAction = unsubscribe.Dispose;

			Interlocked
				.Exchange(ref _processor, processor)
				.Dispose();
		}

		public Task Commit(IAggregateRoot root)
		{
			return DomainBus.RaiseWithTask(root.Flush());
		}
	}
}