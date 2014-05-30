namespace Kostassoid.Nerve.EventStore.Specs.Command
{
	using System;
	using Nerve.EventStore.Command;

	public class CelebrateUserBirthday : ICommand
	{
		public Guid Id { get; private set; }

		public CelebrateUserBirthday(Guid id)
		{
			Id = id;
		}
	}
}