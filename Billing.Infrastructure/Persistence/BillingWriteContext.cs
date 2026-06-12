

using Billing.Application.Common.Abstractions;
using Billing.Domain.Invoices;
using Billing.Infrastructure.Outbox;

using Microsoft.EntityFrameworkCore;

using System.Reflection;

namespace Billing.Infrastructure.Persistence;

public sealed class BillingWriteContext(
    DbContextOptions<BillingWriteContext> options,
    ITenantProvider tenant)
    : DbContext(options), IUnitOfWork, IBillingDbContext
{
    public DbSet<Invoice> Invoices => Set<Invoice>();
    public DbSet<OutboxMessage> OutboxMessages => Set<OutboxMessage>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());

        modelBuilder.Entity<Invoice>().HasQueryFilter(i =>
            EF.Property<bool>(i, "IsDeleted") == false &&
            EF.Property<Guid>(i, "TenantId") == tenant.TenantId);
    }

    public async Task ExecuteTransactionalAsync(Func<Task> operation, CancellationToken ct = default)
    {
        var strategy = Database.CreateExecutionStrategy();   // honors EnableRetryOnFailure
        await strategy.ExecuteAsync(async () =>
        {
            await using var tx = await Database.BeginTransactionAsync(ct);
            await operation();
            await tx.CommitAsync(ct);
        });
    }
}
