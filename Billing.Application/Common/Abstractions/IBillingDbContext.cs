

using Billing.Domain.Invoices;

using Microsoft.EntityFrameworkCore;

namespace Billing.Application.Common.Abstractions;

public interface IBillingDbContext
{
    DbSet<Invoice> Invoices { get; }
}