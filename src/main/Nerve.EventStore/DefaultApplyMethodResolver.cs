namespace Kostassoid.Nerve.EventStore
{
	using System;
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
	}
}