namespace Kostassoid.Nerve.EventStore
{
	using Model;

	public delegate void ApplyMethodDelegate(IAggregateRoot root, IDomainEvent ev);
}