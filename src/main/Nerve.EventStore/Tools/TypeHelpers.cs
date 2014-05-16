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

namespace Kostassoid.Nerve.EventStore.Tools
{
	using System;
	using System.Linq.Expressions;

	public static class TypeHelpers
	{
		static readonly Func<Type, Func<object>> InstanceBuilderFunc
			= MemoizedFunc.From<Type, Func<object>>(type => Expression.Lambda<Func<object>>(Expression.New(type)).Compile());

		public static T New<T>()
		{
			return (T)InstanceBuilderFunc(typeof(T))();
		}

		public static object New(Type type)
		{
			return InstanceBuilderFunc(type)();
		}

	}
}