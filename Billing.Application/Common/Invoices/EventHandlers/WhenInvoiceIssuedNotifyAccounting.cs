using Billing.Domain.Invoices;

using MediatR;

using Microsoft.Extensions.Logging;

namespace Billing.Application.Common.Invoices.EventHandlers;

public sealed class WhenInvoiceIssuedNotifyAccounting(ILogger<WhenInvoiceIssuedNotifyAccounting> logger)
    : INotificationHandler<InvoiceIssued>
{
    public Task Handle(InvoiceIssued e, CancellationToken ct)
    {

        logger.LogInformation("Invoice {Id} issued for {Amount} {Currency}", e.Id.Value, e.Amount, e.Currency);

        return Task.CompletedTask;
    }
}
