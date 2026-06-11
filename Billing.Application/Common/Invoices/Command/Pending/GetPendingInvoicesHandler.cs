using Billing.Application.Common.Abstractions;
using Billing.Application.Common.Invoices.Dto;
using Billing.Domain.Invoices;

using MediatR;

using Microsoft.EntityFrameworkCore;

namespace Billing.Application.Common.Invoices.Command.Pending;

public sealed class GetPendingInvoicesHandler(IReadDbContext read)
    : IRequestHandler<GetPendingInvoicesQuery, IReadOnlyList<InvoiceDto>>
{
    public async Task<IReadOnlyList<InvoiceDto>> Handle(GetPendingInvoicesQuery _, CancellationToken ct) =>
        await read.Invoices
            .Where(i => i.Status == InvoiceStatus.Issued)
            .Select(i => new InvoiceDto(
                i.Id.Value, 
                i.Number.Value, 
                i.Total.Amount, 
                i.Total.Currency, 
                i.Status.ToString()))
            .ToListAsync(ct);
}
