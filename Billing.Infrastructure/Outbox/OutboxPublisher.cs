using Billing.Domain.Common;
using Billing.Infrastructure.Persistence;

using MediatR;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

using System.Text.Json;

namespace Billing.Infrastructure.Outbox;

public sealed class OutboxPublisher(
    IServiceScopeFactory scopeFactory,
    ILogger<OutboxPublisher> logger) : BackgroundService
{
    private const int BatchSize = 20;
    private const int MaxAttempts = 5;
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try { await ProcessBatchAsync(stoppingToken); }
            catch (Exception ex) { logger.LogError(ex, "Outbox processing failed."); }

            await Task.Delay(TimeSpan.FromSeconds(2), stoppingToken);
        }
    }

    private async Task ProcessBatchAsync(CancellationToken ct)
    {
        using var scope = scopeFactory.CreateScope();
        var ctx = scope.ServiceProvider.GetRequiredService<BillingWriteContext>();
        var publisher = scope.ServiceProvider.GetRequiredService<IPublisher>();

        var batch = await ctx.OutboxMessages
            .Where(m => m.ProcessedOn == null && m.Attempts < MaxAttempts)
            .OrderBy(m => m.OccurredOn)
            .Take(BatchSize)
            .ToListAsync(ct);

        if (batch.Count == 0) return;

        foreach (var message in batch)
        {
            try
            {
                var type = Type.GetType(message.Type, throwOnError: true);
                var @event = (IDomainEvent)JsonSerializer.Deserialize(message.Payload, type)!;
                await publisher.Publish(@event, ct);
                message.ProcessedOn = DateTime.UtcNow;
            }
            catch (Exception ex)
            {
                message.Attempts++;
                message.Error = ex.Message;
                logger.LogWarning(ex, "Failed to publish outbox message {Id} (attempt {Attempt})",
                    message.Id, message.Attempts);
            }
        }
        
        await ctx.SaveChangesAsync(ct);
    }
}
