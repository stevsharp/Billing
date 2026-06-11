namespace Billing.Application.Common.Invoices.Dto;

public sealed record InvoiceLineDto(string Description, decimal Amount);
