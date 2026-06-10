using MediatR;

namespace Billing.Domain.Common;

public interface IDomainEvent : INotification;

public interface IHasDomainEvents
{
    public List<IDomainEvent> DomainEvents { get; }

    void ClearDomainEvents();
}

public abstract class Entity<TId> where TId : struct
{
    public TId Id { get; protected set; }

    public override bool Equals(object? obj) =>
        obj is Entity<TId> other && other.GetType() == GetType() && other.Id.Equals(Id);

    public override int GetHashCode() => base.GetHashCode();

}