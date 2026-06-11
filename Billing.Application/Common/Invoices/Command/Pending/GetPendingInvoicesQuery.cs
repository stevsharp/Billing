using Billing.Application.Common.Invoices.Dto;

using MediatR;

namespace Billing.Application.Common.Invoices.Command.Pending;

public sealed record GetPendingInvoicesQuery : IRequest<IReadOnlyList<InvoiceDto>>;
