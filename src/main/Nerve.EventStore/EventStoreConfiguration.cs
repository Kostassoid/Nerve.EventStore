namespace Kostassoid.Nerve.EventStore
{
	using Core;
	using Storage;

	internal class EventStoreConfiguration : IEventStoreConfigurator
	{
		public ICell Source { get; private set; }
		public ICell Target { get; private set; }
		public IEventStorage Storage { get; private set; }

		public void ListenTo(ICell source)
		{
			Source = source;
		}

		public void BroadcastTo(ICell target)
		{
			Target = target;
		}

		public void UseStorage(IEventStorage storage)
		{
			Storage = storage;
		}
	}
}