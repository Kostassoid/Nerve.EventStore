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
	using Fasterflect;
	using Tools;
	using Tools.CodeContracts;

	public static class DomainSettings
	{
		public static IApplyMethodResolver ApplyMethodResolver { get; set; }

		static DomainSettings()
		{
			ApplyMethodResolver = new DefaultApplyMethodResolver();
		}

		internal static void Apply(IAggregateRoot root, IDomainEvent ev)
		{
			ResolveApplyMethodDelegate(root.GetType(), ev.GetType())(root, ev);
		}

		static ApplyMethodDelegate BuildApplyMethod(Type rootType, Type eventType)
		{
			Requires.ValidState(ApplyMethodResolver != null, "Apply method resolver is not set.");

			// ReSharper disable once PossibleNullReferenceException
			var mi = ApplyMethodResolver.Resolve(rootType, eventType);
			Assumes.True(mi != null, "Apply method for event [{0}] wasn't resolved in [{1}]", eventType.Name, rootType.Name);

			var invoker = mi.DelegateForCallMethod();
			return (root, ev) => invoker(root, new object[] {ev});
		}

		static readonly Func<Type, Type, ApplyMethodDelegate> ResolveApplyMethodDelegate =
			MemoizedFunc.From<Type, Type, ApplyMethodDelegate>(BuildApplyMethod);
	}
}