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

    public static Invoice Create(InvoiceNumber number, string currency)
    {
        var invoice = new Invoice
        {
            Id = InvoiceId.New(),
            Number = number,
            Status = InvoiceStatus.Draft,
            Total = Money.Zero(currency)
        };
        invoice.Raise(new InvoiceCreated(invoice.Id, number.Value));
        return invoice;
    }

    public void AddLine(string description, Money lineTotal)
    {
        ArgumentNullException.ThrowIfNull(description);

        if (Status != InvoiceStatus.Draft)
            throw new DomainException("Can only add lines to a draft invoice.");

        _lines.Add(new InvoiceLine(description, lineTotal));

        Total = Total.Add(lineTotal);
    }

    public void Issue()
    {
        if (Status != InvoiceStatus.Draft) 
            throw new DomainException("Only drafts can be issued.");

        if (_lines.Count == 0) 
            throw new DomainException("Cannot issue an empty invoice.");

        Status = InvoiceStatus.Issued;
        Raise(new InvoiceIssued(Id, Total.Amount, Total.Currency));
    }

    public void Pay()
    {
        if (Status != InvoiceStatus.Issued) 
            throw new DomainException("Only issued invoices can be paid.");

        Status = InvoiceStatus.Paid;
        Raise(new InvoicePaid(Id));
    }

    public void Cancel(string reason)
    {
        if (Status == InvoiceStatus.Paid) 
            throw new DomainException("Cannot cancel a paid invoice.");

        Status = InvoiceStatus.Cancelled;
        Raise(new InvoiceCancelled(Id, reason));
    }

}