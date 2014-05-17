namespace Kostassoid.Nerve.EventStore.Specs
{
	using System;
	using System.Collections.Concurrent;
	using System.Collections.Generic;
	using System.Linq;
	using Storage;

	public class InMemoryEventStorage : IEventStorage
    {
		private readonly IDictionary<Guid, IList<Commit>> _storage =
			new ConcurrentDictionary<Guid, IList<Commit>>();

		public Commit LoadLast(Guid id)
		{
			IList<Commit> commits;
			if (!_storage.TryGetValue(id, out commits))
			{
				return null;
			}
			
			return commits.Last();
		}

		public IEnumerable<Commit> LoadSince(Guid id, long startingId)
		{
			IList<Commit> commits;
			if (!_storage.TryGetValue(id, out commits))
			{
				return null;
			}

			return commits.Skip((int)startingId);
		}

		public void Store(Commit commit)
		{
			IList<Commit> commits;
			if (!_storage.TryGetValue(commit.TargetId, out commits))
			{
				commits = new List<Commit>();
			}

			commits.Add(commit);

			_storage[commit.TargetId] = commits;
		}
    }
}
