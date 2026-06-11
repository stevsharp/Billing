using Billing.Application.Common.Abstractions;
using Billing.Domain.Invoices;

using MediatR;

namespace Billing.Application.Common.Invoices.Command.Create;

public sealed class IssueInvoiceHandler(IBillingDbContext db) : IRequestHandler<IssueInvoiceCommand, Guid>
{
    public async Task<Guid> Handle(IssueInvoiceCommand cmd, CancellationToken ct)
    {
        var invoice = Invoice.Create(new InvoiceNumber(cmd.Number), cmd.Currency);
        foreach (var line in cmd.Lines)
            invoice.AddLine(line.Description, new Money(line.Amount, cmd.Currency));
        invoice.Issue();

        await db.AddInvoiceAsync(invoice, ct);

        return invoice.Id.Value;  
    }
}