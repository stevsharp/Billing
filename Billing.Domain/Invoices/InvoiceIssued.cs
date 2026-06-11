using Billing.Domain.Common;

namespace Billing.Domain.Invoices;

public sealed record InvoiceIssued(InvoiceId Id, decimal Amount, string Currency) : IDomainEvent;
