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
	using Model;

	public class ConcurrencyException : Exception
	{
		public ConcurrencyException(IAggregateRoot root, IDomainEvent conflictEvent)
			: base(string.Format("Expected version of {0}({1}) to be {2} but got {3}."
			, root.GetType().Name, root.Id, root.Version, conflictEvent.Version))
		{
		}
	}
}