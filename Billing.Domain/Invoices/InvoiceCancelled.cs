using Billing.Domain.Common;

namespace Billing.Domain.Invoices;

public sealed record InvoiceCancelled(InvoiceId Id, string Reason) : IDomainEvent;