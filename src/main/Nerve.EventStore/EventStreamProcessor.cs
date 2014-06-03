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
	using Tools;

	internal class EventStreamProcessor : Cell
	{
		readonly IEventStorage _storage;

		public EventStreamProcessor(IEventStorage storage) : base("EventStoreProcessor", ThreadScheduler.Factory)
		{
			_storage = storage;

			OnStream().Of<UncommitedEventStream>().ReactWith(ProcessUncommited);
			OnStream().Of<AggregateIdentity>().ReactWith(Load);
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
					throw new ConcurrencyException(signal.Payload.Root, ev);
				}

				commited.Add(ev);
				currentVersion++;
			}

			var commitId = last != null ? last.Id + 1 : 0;
			var snapshot = last != null ? last.Snapshot : null;
			var snapshotId = last != null ? last.SnapshotId : null;

			if (root is ISnapshotEnabled && last != null && ShouldBuildSnapshot(last))
			{
				snapshot = (root as ISnapshotEnabled).BuildSnapshot();
				snapshotId = commitId;
			}

			if (commited.Any())
			{
				var commit = new Commit(
					commitId,
					root.Id,
					targetVersion,
					commited,
					snapshotId,
					snapshot
					);

				_storage.Store(commit);
			}

			signal.Return(uncommited.Events);
		}

		bool ShouldBuildSnapshot(Commit last)
		{
			return last.Id - (last.SnapshotId.HasValue ? last.SnapshotId.Value : 0) > 10;
		}

		void Load(ISignal<AggregateIdentity> signal)
		{
			var id = signal.Payload.Id;
			var type = signal.Payload.Type;

			var last = _storage.LoadLast(id);
			if (last == null)
			{
				throw new InvalidOperationException(string.Format("Aggregate root of type {0} with id {1} not found.", type.Name, id));
			}

			var root = (IAggregateRoot)TypeHelpers.New(type);

			var loadId = last.SnapshotId.HasValue ? last.SnapshotId.Value : 0;

			foreach (var commit in _storage.LoadSince(id, loadId))
			{
				if (commit.Snapshot != null)
				{
					root.Apply(commit.Snapshot, true);
					continue;
				}

				foreach (var ev in commit.Events)
				{
					root.Apply(ev, true);
				}
			}

			signal.Return(root);
		}


	}
}