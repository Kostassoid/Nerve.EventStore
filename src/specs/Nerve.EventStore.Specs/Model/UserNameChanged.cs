﻿namespace Kostassoid.Nerve.EventStore.Specs.Model
{
	using Nerve.EventStore.Model;

	public class UserNameChanged : DomainEvent
	{
		public string NewName { get; set; }

		public UserNameChanged()
		{}

		public UserNameChanged(AggregateRoot root, string newName) : base(root)
		{
			NewName = newName;
		}
	}

}