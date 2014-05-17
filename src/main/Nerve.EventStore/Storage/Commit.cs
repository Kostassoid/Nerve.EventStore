namespace Kostassoid.Nerve.EventStore.Storage
{
	using System;
	using System.Collections.Generic;
	using Model;

	public class Commit
	{
		public long Id { get; private set; }

		public Guid TargetId { get; private set; }

		public long TargetVersion { get; private set; }

		public IList<IDomainEvent> Events { get; private set; }

		public long? SnapshotId { get; private set; }

		public ISnapshot Snapshot { get; private set; }

		public Commit(long id, Guid targetId, long targetVersion, IList<IDomainEvent> events, long? snapshotId = null, ISnapshot snapshot = null)
		{
			Id = id;
			TargetId = targetId;
			TargetVersion = targetVersion;
			Events = events;
			SnapshotId = snapshotId;
			Snapshot = snapshot;
		}
	}
}