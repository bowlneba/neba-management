namespace Neba.Domain;

public abstract class AggregateRoot<TId>
    : Entity<TId>, IAggregateRoot
where TId : struct, IEquatable<TId>
{
    private readonly List<IDomainEvent> _domainEvents = [];

    protected AggregateRoot(TId id)
        : base(id)
    {
    }

    public IReadOnlyCollection<IDomainEvent> PopDomainEvents()
    {
        var domainEvents = _domainEvents.ToList();
        _domainEvents.Clear();

        return domainEvents;
    }

    protected void AddDomainEvent(IDomainEvent domainEvent)
        => _domainEvents.Add(domainEvent);
}

public interface IAggregateRoot
{
    IReadOnlyCollection<IDomainEvent> PopDomainEvents();
}
