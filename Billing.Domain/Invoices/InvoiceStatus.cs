namespace Billing.Domain.Invoices;

public enum InvoiceStatus
{
    Draft,
    Issued,
    Paid,
    Cancelled
}