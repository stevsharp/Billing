

namespace Billing.Application.Common.Abstractions;

public interface IUnitOfWork
{
    Task<int> SaveChangesAsync(CancellationToken ct = default);
    Task ExecuteTransactionalAsync(Func<Task> operation, CancellationToken ct = default);
}