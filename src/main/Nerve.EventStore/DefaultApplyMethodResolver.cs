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
	using System.Reflection;

	public class DefaultApplyMethodResolver : IApplyMethodResolver
	{
		public MethodInfo Resolve(Type rootType, Type eventType)
		{
			return rootType.GetMethod(
				"On" + eventType.Name,
				BindingFlags.Instance | BindingFlags.NonPublic /*| BindingFlags.InvokeMethod*/,
				null, new[] { eventType }, null);

			//return rootType.GetMethod("On" + eventType.Name, new[] {eventType});
		}

		public IEnumerable<MethodInfo> ResolveAll(Type rootType)
		{
			return rootType.GetMethods(BindingFlags.Instance | BindingFlags.NonPublic)
				.Where(m => m.Name.StartsWith("On") && m.GetParameters().Count() == 1);
		}
	}
}