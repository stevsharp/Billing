using Billing.Domain.Invoices;

namespace Billing.Application.Common.Abstractions;

public interface IReadDbContext
{
    IQueryable<Invoice> Invoices { get; }
}