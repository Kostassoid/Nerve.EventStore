namespace Kostassoid.Nerve.EventStore.Specs.Model
{
	using System;
	using Nerve.EventStore.Model;

	public class User : AggregateRoot, ISnapshotEnabled
	{
		public string Name { get; private set; }
		public int Age { get; private set; }

		protected User()
		{ }

		protected User(Guid id)
			: base(id)
		{ }

		public static User Create(Guid id, string name, int age)
		{
			var user = new User(id);
			user.Apply(new UserCreated(user, name, age));
			return user;
		}

		protected void OnUserCreated(UserCreated ev)
		{
			Id = ev.Id;
			Name = ev.Name;
			Age = ev.Age;
		}

		public void ChangeName(string newName)
		{
			if (string.IsNullOrWhiteSpace(newName))
			{
				throw new ArgumentException("Name shouldn't be empty.", "newName");
			}

			Apply(new UserNameChanged(this, newName));
		}

		protected void OnUserNameChanged(UserNameChanged ev)
		{
			Name = ev.NewName;
		}

		public void Birthday()
		{
			Apply(new UserAgeChanged(this, Age + 1));
		}

		protected void OnUserAgeChanged(UserAgeChanged ev)
		{
			Age = ev.NewAge;
		}

		ISnapshot ISnapshotEnabled.BuildSnapshot()
		{
			return new UserSnapshot(this, Name, Age);
		}

		protected void OnUserSnapshot(UserSnapshot snapshot)
		{
			Id = snapshot.Id;
			Version = snapshot.Version;
			Name = snapshot.Name;
			Age = snapshot.Age;
		}
	}
}