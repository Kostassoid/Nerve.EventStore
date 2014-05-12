namespace Kostassoid.Nerve.EventStore.Specs
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using Nerve.EventStore.Model;
	using Storage;

	public class InMemoryEventStorage : IEventStorage
    {
		private readonly IDictionary<Guid, IList<IDomainEvent>> _storage =
			new Dictionary<Guid, IList<IDomainEvent>>(); 

		public IEnumerable<IDomainEvent> Load(Type type, Guid id)
		{
			IList<IDomainEvent> events;
			if (!_storage.TryGetValue(id, out events))
			{
				return new IDomainEvent[0];
			}

			return events;
		}

		public void Save(Type type, Guid id, IEnumerable<IDomainEvent> events)
		{
			_storage[id] = events.ToList();
		}
    }
}
