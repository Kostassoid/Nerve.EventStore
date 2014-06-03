namespace Kostassoid.Nerve.EventStore.Specs.Model
{
	using System;
	using Nerve.EventStore.Model;

	public class UserSnapshot : ISnapshot
	{
		public Guid Id { get; private set; }
		public long Version { get; private set; }
		public string Name { get; private set; }
		public int Age { get; private set; }

		protected UserSnapshot() {}

		public UserSnapshot(IAggregateRoot root, string name, int age)
		{
			Id = root.Id;
			Version = root.Version;
			Name = name;
			Age = age;
		}
	}
}