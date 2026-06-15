using Billing.Domain.Common;
using Billing.Domain.Invoices;

using FluentAssertions;

namespace Billing.Tests.Domain;

public class InvoiceTests
{
    [Fact]
    public void Create_ReturnsDraftWithZeroTotal()
    {
        var invoice = Invoice.Create(new InvoiceNumber("INV-001"), "EUR");

        invoice.Status.Should().Be(InvoiceStatus.Draft);
        invoice.Total.Amount.Should().Be(0);
        invoice.Total.Currency.Should().Be("EUR");
        invoice.Lines.Should().BeEmpty();
    }

    [Fact]
    public void Create_RaisesInvoiceCreatedEvent()
    {
        var invoice = Invoice.Create(new InvoiceNumber("INV-001"), "EUR");

        invoice.DomainEvents.Should().ContainSingle()
            .Which.Should().BeOfType<InvoiceCreated>();
    }

    [Fact]
    public void AddLine_ToDraft_UpdatesTotal()
    {
        var invoice = Invoice.Create(new InvoiceNumber("INV-001"), "EUR");

        invoice.AddLine("Consulting", new Money(150m, "EUR"));

        invoice.Lines.Should().HaveCount(1);
        invoice.Total.Amount.Should().Be(150m);
    }

    [Fact]
    public void AddLine_MultipleLines_AccumulatesTotal()
    {
        var invoice = Invoice.Create(new InvoiceNumber("INV-001"), "EUR");

        invoice.AddLine("Line 1", new Money(100m, "EUR"));
        invoice.AddLine("Line 2", new Money(250m, "EUR"));

        invoice.Total.Amount.Should().Be(350m);
        invoice.Lines.Should().HaveCount(2);
    }

    [Fact]
    public void AddLine_ToIssuedInvoice_ThrowsDomainException()
    {
        var invoice = Invoice.Create(new InvoiceNumber("INV-001"), "EUR");
        invoice.AddLine("Service", new Money(100m, "EUR"));
        invoice.Issue();

        var act = () => invoice.AddLine("Extra", new Money(50m, "EUR"));

        act.Should().ThrowExactly<DomainException>().WithMessage("*draft*");
    }

    [Fact]
    public void Issue_WithLines_SetsStatusToIssued()
    {
        var invoice = Invoice.Create(new InvoiceNumber("INV-001"), "EUR");
        invoice.AddLine("Service", new Money(100m, "EUR"));

        invoice.Issue();

        invoice.Status.Should().Be(InvoiceStatus.Issued);
    }

    [Fact]
    public void Issue_WithLines_RaisesInvoiceIssuedEvent()
    {
        var invoice = Invoice.Create(new InvoiceNumber("INV-001"), "EUR");
        invoice.AddLine("Service", new Money(200m, "EUR"));

        invoice.Issue();

        invoice.DomainEvents.OfType<InvoiceIssued>().Should().ContainSingle();
    }

    [Fact]
    public void Issue_NoLines_ThrowsDomainException()
    {
        var invoice = Invoice.Create(new InvoiceNumber("INV-001"), "EUR");

        var act = () => invoice.Issue();

        act.Should().ThrowExactly<DomainException>().WithMessage("*empty*");
    }

    [Fact]
    public void Issue_AlreadyIssued_ThrowsDomainException()
    {
        var invoice = Invoice.Create(new InvoiceNumber("INV-001"), "EUR");
        invoice.AddLine("Service", new Money(100m, "EUR"));
        invoice.Issue();

        var act = () => invoice.Issue();

        act.Should().ThrowExactly<DomainException>().WithMessage("*drafts*");
    }

    [Fact]
    public void Pay_IssuedInvoice_SetsStatusToPaid()
    {
        var invoice = Invoice.Create(new InvoiceNumber("INV-001"), "EUR");
        invoice.AddLine("Service", new Money(100m, "EUR"));
        invoice.Issue();

        invoice.Pay();

        invoice.Status.Should().Be(InvoiceStatus.Paid);
    }

    [Fact]
    public void Pay_IssuedInvoice_RaisesInvoicePaidEvent()
    {
        var invoice = Invoice.Create(new InvoiceNumber("INV-001"), "EUR");
        invoice.AddLine("Service", new Money(100m, "EUR"));
        invoice.Issue();

        invoice.Pay();

        invoice.DomainEvents.OfType<InvoicePaid>().Should().ContainSingle();
    }

    [Fact]
    public void Pay_DraftInvoice_ThrowsDomainException()
    {
        var invoice = Invoice.Create(new InvoiceNumber("INV-001"), "EUR");

        var act = () => invoice.Pay();

        act.Should().ThrowExactly<DomainException>().WithMessage("*issued*");
    }

    [Fact]
    public void Cancel_DraftInvoice_SetsCancelled()
    {
        var invoice = Invoice.Create(new InvoiceNumber("INV-001"), "EUR");

        invoice.Cancel("Changed mind");

        invoice.Status.Should().Be(InvoiceStatus.Cancelled);
    }

    [Fact]
    public void Cancel_IssuedInvoice_SetsCancelled()
    {
        var invoice = Invoice.Create(new InvoiceNumber("INV-001"), "EUR");
        invoice.AddLine("Service", new Money(100m, "EUR"));
        invoice.Issue();

        invoice.Cancel("Client request");

        invoice.Status.Should().Be(InvoiceStatus.Cancelled);
    }

    [Fact]
    public void Cancel_PaidInvoice_ThrowsDomainException()
    {
        var invoice = Invoice.Create(new InvoiceNumber("INV-001"), "EUR");
        invoice.AddLine("Service", new Money(100m, "EUR"));
        invoice.Issue();
        invoice.Pay();

        var act = () => invoice.Cancel("Mistake");

        act.Should().ThrowExactly<DomainException>().WithMessage("*paid*");
    }

    [Fact]
    public void Cancel_RaisesInvoiceCancelledEvent()
    {
        var invoice = Invoice.Create(new InvoiceNumber("INV-001"), "EUR");

        invoice.Cancel("Reason");

        invoice.DomainEvents.OfType<InvoiceCancelled>().Should().ContainSingle();
    }
}
