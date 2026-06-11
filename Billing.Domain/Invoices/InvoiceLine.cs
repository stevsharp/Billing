using Billing.Domain.Common;

namespace Billing.Domain.Invoices;

public sealed class  InvoiceLine : Entity<Guid>
{
    public string Description { get; private set; }
    public Money Amount { get; private set; }

    internal InvoiceLine(string description, Money amount)
    {
        Id = Guid.NewGuid();
        Description = description;
        Amount = amount;
    }

    /// <summary>
    /// Only for EF Core
    /// </summary>
    private InvoiceLine() 
    {
        Description = default!;
        Amount = default!;
    }
}