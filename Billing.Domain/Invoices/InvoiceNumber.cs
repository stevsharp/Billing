namespace Billing.Domain.Invoices;

public sealed record InvoiceNumber
{
    public string Value { get; init; }
    public InvoiceNumber(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Invoice number cannot be null or whitespace.", nameof(value));
        Value = value;
    }
    public override string ToString() => Value;
}