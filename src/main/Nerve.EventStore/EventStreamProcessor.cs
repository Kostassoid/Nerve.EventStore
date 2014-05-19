namespace Kostassoid.Nerve.EventStore
{
	using System;
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

		public EventStreamProcessor(IEventStorage storage) : base("EventStoreProcessor", ThreadScheduler.Factory)
		{
			_storage = storage;

			OnStream().Of<UncommitedEventStream>().ReactWith(ProcessUncommited);
		}

		void ProcessUncommited(ISignal<UncommitedEventStream> signal)
		{
			var uncommited = signal.Payload;
			var root = uncommited.Root;
			if (!uncommited.Events.Any())
			{
				signal.Return(new IDomainEvent[0]);
				return;
			}

			var last = _storage.LoadLast(root.Id);
			var toCommit = uncommited.Events;
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

			signal.Return(uncommited.Events);
		}
	}
}