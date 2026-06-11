using Billing.Application.Common.Abstractions;
using Billing.Domain.Invoices;

using MediatR;

namespace Billing.Application.Common.Invoices.Command.Cancel;

public sealed class CancelInvoiceHandler(IBillingDbContext db) : IRequestHandler<CancelInvoiceCommand>
{
    public async Task Handle(CancelInvoiceCommand cmd, CancellationToken ct)
    {
        var invoice = await db.GetInvoiceAsync(new InvoiceId(cmd.InvoiceId), ct);
        invoice.Cancel(cmd.Reason);
    }
}
