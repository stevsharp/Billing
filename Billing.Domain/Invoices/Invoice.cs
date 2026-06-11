using Billing.Domain.Common;

namespace Billing.Domain.Invoices;

public sealed class Invoice : AggregateRoot<InvoiceId>
{
    private readonly List<InvoiceLine> _lines = [];
    public IReadOnlyCollection<InvoiceLine> Lines => _lines.AsReadOnly();

    public InvoiceNumber Number { get; private set; } = default!;
    public InvoiceStatus Status { get; private set; }
    public Money Total { get; private set; } = default!;

    private Invoice(){}

    public static Invoice Create(InvoiceNumber number, InvoiceStatus status, Money total)
    {
        var invoice = new Invoice
        {
            Id = InvoiceId.New(),
            Number = number,
            Status = status,
            Total = total
        };

        // raise domain events if needed, e.g. InvoiceCreated

        invoice.Raise(new InvoiceCreated(invoice.Id, number.Value));

        return invoice;
    }

}