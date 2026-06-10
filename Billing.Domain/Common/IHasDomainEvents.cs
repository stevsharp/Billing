namespace Billing.Domain.Common;

public interface IHasDomainEvents
{
    public List<IDomainEvent> DomainEvents { get; }

    void ClearDomainEvents();
}
