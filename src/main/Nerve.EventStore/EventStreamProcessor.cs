namespace Kostassoid.Nerve.EventStore
{
	using System.Collections.Generic;
	using System.Linq;
	using Core;
	using Core.Processing.Operators;
	using Core.Scheduling;
	using Model;
	using Storage;

	internal class EventStreamProcessor : Cell
	{
		readonly IEventStorage _storage;

		public EventStreamProcessor(IEventStorage storage) : base("EventStoreProcessor", PoolScheduler.Factory)
		{
			_storage = storage;

			OnStream().Cast<IAggregateRoot>().ReactWith(ProcessUncommited); //TODO: use Of<>
		}

		void ProcessUncommited(ISignal<IAggregateRoot> signal)
		{
			var root = signal.Payload;
			var uncommited = root.Flush();
			if (!uncommited.UncommitedEvents.Any())
			{
				signal.Return(new IDomainEvent[0]);
			}

			var last = _storage.LoadLast(root.Id);
			var toCommit = uncommited.UncommitedEvents;
			var commited = new List<IDomainEvent>();

			var targetVersion = last != null ? last.TargetVersion + last.Events.Count : 0;
			var currentVersion = targetVersion;
			foreach (var ev in toCommit)
			{
				if (currentVersion != ev.Version)
				{
					throw new ConcurrencyException(currentVersion, ev);
				}

				commited.Add(ev);
				currentVersion++;
			}

			if (commited.Any())
			{
				var commit = new Commit(
					last != null ? last.Id + 1 : 0,
					root.Id,
					targetVersion,
					commited
					);

				_storage.Store(commit);
			}

			signal.Return(uncommited.UncommitedEvents);
		}
	}
}