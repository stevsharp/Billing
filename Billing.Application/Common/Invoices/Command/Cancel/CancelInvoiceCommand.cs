using Billing.Application.Common.Behaviors;

using MediatR;

namespace Billing.Application.Common.Invoices.Command.Cancel;

public sealed record CancelInvoiceCommand(Guid InvoiceId, string Reason)
    : IRequest, ITransactionalRequest;
