

using Billing.Application.Common.Abstractions;
using Billing.Domain.Invoices;

using Microsoft.EntityFrameworkCore;

using System.Reflection;

namespace Billing.Infrastructure.Persistence;

public sealed class BillingReadContext(
    DbContextOptions<BillingReadContext> options,
    ITenantProvider tenant) : DbContext(options), IReadDbContext
{
    IQueryable<Invoice> IReadDbContext.Invoices => Set<Invoice>().AsNoTracking();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());

        modelBuilder.Entity<Invoice>().HasQueryFilter(i =>
            EF.Property<bool>(i, "IsDeleted") == false &&
            EF.Property<Guid>(i, "TenantId") == tenant.TenantId);
    }
}