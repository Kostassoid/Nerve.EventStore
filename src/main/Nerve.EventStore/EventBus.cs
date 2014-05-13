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
	using System.Threading.Tasks;
	using Core;
	using Core.Tpl;

	public static class EventBus
	{
		static ICell _cell = new Cell("EventBus");

		public static ILinkJunction OnStream()
		{
			return _cell.OnStream();
		}

		public static void Raise<T>(T ev) where T : class
		{
			_cell.Send(ev);
		}

		public static Task RaiseWithTask<T>(T ev) where T : class
		{
			return _cell.SendFor<object>(ev);
		}

		public static void Reset()
		{
			var tmp = _cell;
			_cell = new Cell("EventBus");
			tmp.Dispose();
		}
	}
}