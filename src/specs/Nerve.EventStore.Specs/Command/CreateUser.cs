namespace Kostassoid.Nerve.EventStore.Specs.Command
{
	using System;
	using Nerve.EventStore.Command;

	public class CreateUser : ICommand
	{
		public Guid Id { get; private set; }
		public string Name { get; private set; }
		public int Age { get; private set; }

		public CreateUser(Guid id, string name, int age)
		{
			Id = id;
			Name = name;
			Age = age;
		}
	}
}