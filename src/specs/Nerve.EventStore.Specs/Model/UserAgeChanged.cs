namespace Kostassoid.Nerve.EventStore.Specs.Model
{
	using Nerve.EventStore.Model;

	public class UserAgeChanged : DomainEvent
	{
		public int NewAge { get; set; }

		public UserAgeChanged()
		{}

		public UserAgeChanged(AggregateRoot root, int newAge) : base(root)
		{
			NewAge = newAge;
		}
	}

}