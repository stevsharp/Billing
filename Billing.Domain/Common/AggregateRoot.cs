namespace Billing.Domain.Common;

public abstract class AggregateRoot<TId> : Entity<TId>, IHasDomainEvents where TId : struct
{
    /// <summary>
    /// Raises a domain event and adds it to the list of domain events.
    /// </summary>
    private readonly List<IDomainEvent> _domainEvents = [];
    /// <summary>
    ///     
    /// </summary>
    public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();
    /// <summary>
    ///     
    /// </summary>
    /// <param name="event"></param>
    protected void Raise(IDomainEvent @event) => _domainEvents.Add(@event);
    /// <summary>
    /// 
    /// </summary>
    public void ClearDomainEvents() => _domainEvents.Clear();
}
