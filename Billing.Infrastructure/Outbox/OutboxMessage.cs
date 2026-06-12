namespace Billing.Infrastructure.Outbox;

public sealed class OutboxMessage
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public string Type { get; init; } = default!;
    public string Payload { get; init; } = default!;
    public DateTime OccurredOn { get; init; }
    public DateTime? ProcessedOn { get; set; }
    public int Attempts { get; set; }
    public string? Error { get; set; }
}
