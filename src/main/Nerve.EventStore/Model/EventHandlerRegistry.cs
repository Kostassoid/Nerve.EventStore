namespace Kostassoid.Nerve.EventStore.Model
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Reflection;
	using Fasterflect;

	public class EventHandlerRegistry
	{
		private readonly IDictionary<Type, MethodInvoker> _handlers;

		public EventHandlerRegistry(IEnumerable<MethodInfo> methods)
		{
			_handlers = methods.ToDictionary(
				m => m.GetParameters()[0].ParameterType,
				m => m.DelegateForCallMethod());
		}

		public void Handle(IAggregateRoot root, IDomainEvent ev)
		{
			MethodInvoker invoker;
			if (!_handlers.TryGetValue(ev.GetType(), out invoker))
			{
				throw new InvalidOperationException(string.Format("Apply method for event [{0}] wasn't resolved in [{1}]", ev.GetType().Name, root.GetType().Name));
			}

			invoker.Invoke(root, new object[] {ev});
		}
	}
}