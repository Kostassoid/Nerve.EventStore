namespace Kostassoid.Nerve.EventStore
{
	using System;
	using System.Collections.Generic;
	using System.Reflection;

	public interface IApplyMethodResolver
	{
		MethodInfo Resolve(Type rootType, Type eventType);
		IEnumerable<MethodInfo> ResolveAll(Type rootType);
	}
}