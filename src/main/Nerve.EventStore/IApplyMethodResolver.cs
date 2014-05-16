namespace Kostassoid.Nerve.EventStore
{
	using System;
	using System.Reflection;

	public interface IApplyMethodResolver
	{
		MethodInfo Resolve(Type rootType, Type eventType);
	}
}