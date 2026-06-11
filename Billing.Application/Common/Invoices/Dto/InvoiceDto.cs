namespace Billing.Application.Common.Invoices.Dto;

public sealed record InvoiceDto(Guid Id, string Number, decimal Total, string Currency, string Status);
