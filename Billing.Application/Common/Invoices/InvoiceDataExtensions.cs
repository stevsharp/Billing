using Billing.Application.Common.Abstractions;
using Billing.Domain.Common;
using Billing.Domain.Invoices;

using Microsoft.EntityFrameworkCore;

namespace Billing.Application.Common.Invoices;

public static class InvoiceDataExtensions
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="db"></param>
    /// <param name="id"></param>
    /// <param name="ct"></param>
    /// <returns></returns>
    public static Task<Invoice?> FindInvoiceAsync(
        this IBillingDbContext db, InvoiceId id, CancellationToken ct = default) =>
        db.Invoices.Include(i => i.Lines).FirstOrDefaultAsync(i => i.Id == id, ct);
    /// <summary>
    /// 
    /// </summary>
    /// <param name="db"></param>
    /// <param name="id"></param>
    /// <param name="ct"></param>
    /// <returns></returns>
    /// <exception cref="DomainException"></exception>
    public static async Task<Invoice> GetInvoiceAsync(
        this IBillingDbContext db, InvoiceId id, CancellationToken ct = default) =>
        await db.FindInvoiceAsync(id, ct)
        ?? throw new DomainException($"Invoice {id.Value} not found.");
    /// <summary>
    /// 
    /// </summary>
    /// <param name="db"></param>
    /// <param name="invoice"></param>
    /// <param name="ct"></param>
    /// <returns></returns>
    public static async Task AddInvoiceAsync(
        this IBillingDbContext db, Invoice invoice, CancellationToken ct = default) =>
        await db.Invoices.AddAsync(invoice, ct);
    /// <summary>
    /// 
    /// </summary>
    /// <param name="db"></param>
    /// <param name="invoice"></param>
    public static void RemoveInvoice(this IBillingDbContext db, Invoice invoice) =>
        db.Invoices.Remove(invoice);   // → soft delete via the interceptor
}
