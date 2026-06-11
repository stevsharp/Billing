using Billing.Domain.Common;

namespace Billing.Domain.Invoices;

public sealed record InvoicePaid(InvoiceId Id) : IDomainEvent;
