using Billing.Application.Common.Behaviors;
using Billing.Application.Common.Invoices.Dto;

using MediatR;

namespace Billing.Application.Common.Invoices.Command.Create;

public sealed record IssueInvoiceCommand(string Number, string Currency , 
        IReadOnlyList<InvoiceLineDto> Lines) : IRequest<Guid>, ITransactionalRequest;
