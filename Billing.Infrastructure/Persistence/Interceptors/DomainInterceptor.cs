using Billing.Application.Common.Abstractions;
using Billing.Domain.Common;
using Billing.Infrastructure.Outbox;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace Billing.Infrastructure.Persistence.Interceptors;

public sealed class DomainInterceptor(ICurrentUser user,ITenantProvider tenant,
    TimeProvider clock) : SaveChangesInterceptor
{
    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
       DbContextEventData eventData, InterceptionResult<int> result, CancellationToken ct = default)
    {
        var ctx = eventData.Context!;
        StampShadowProperties(ctx);
        WriteDomainEventsToOutbox(ctx);
        return base.SavingChangesAsync(eventData, result, ct);
    }

    public override InterceptionResult<int> SavingChanges(
       DbContextEventData eventData, InterceptionResult<int> result)
    {
        var ctx = eventData.Context!;
        StampShadowProperties(ctx);
        WriteDomainEventsToOutbox(ctx);
        return base.SavingChanges(eventData, result);
    }
    private void StampShadowProperties(DbContext ctx)
    {
        var now = clock.GetUtcNow().UtcDateTime;
        var who = user.UserId;

        foreach (var entry in ctx.ChangeTracker.Entries())
        {
            bool Has(string p) => entry.Metadata.FindProperty(p) is not null;

            switch (entry.State)
            {
                case EntityState.Added:
                    if (Has("TenantId")) 
                        entry.Property("TenantId").CurrentValue = tenant.TenantId;
                    if (Has("CreatedOn")) 
                        entry.Property("CreatedOn").CurrentValue = now;
                    if (Has("CreatedBy")) 
                        entry.Property("CreatedBy").CurrentValue = who;
                    break;

                case EntityState.Modified:
                    if (Has("ModifiedOn")) 
                        entry.Property("ModifiedOn").CurrentValue = now;
                    if (Has("ModifiedBy")) 
                        entry.Property("ModifiedBy").CurrentValue = who;
                    break;

                case EntityState.Deleted when Has("IsDeleted"):
                    entry.State = EntityState.Modified;          // soft delete
                    entry.Property("IsDeleted").CurrentValue = true;
                    entry.Property("DeletedOn").CurrentValue = now;
                    entry.Property("DeletedBy").CurrentValue = who;
                    break;
            }
        }
    }

    private void WriteDomainEventsToOutbox(DbContext ctx)
    {
        var now = clock.GetUtcNow().UtcDateTime;

        var aggregates = ctx.ChangeTracker.Entries<IHasDomainEvents>()
            .Where(e => e.Entity.DomainEvents.Any())
            .Select(e => e.Entity)
            .ToList();

        var messages = aggregates
            .SelectMany(a => a.DomainEvents)
            .Select(e => new OutboxMessage
            {
                Id = Guid.NewGuid(),
                Type = e.GetType().FullName!,
                Payload = System.Text.Json.JsonSerializer.Serialize(e, e.GetType()),
                OccurredOn = now
            })
            .ToList();

        aggregates.ForEach(a => a.ClearDomainEvents());

        if (messages.Any())
        {
            ctx.Set<OutboxMessage>().AddRange(messages);
        }

    }
}
