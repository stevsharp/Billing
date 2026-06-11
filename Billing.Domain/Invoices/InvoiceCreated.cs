using Billing.Domain.Common;

namespace Billing.Domain.Invoices;

public sealed record InvoiceCreated(InvoiceId Id, string Number) : IDomainEvent;
