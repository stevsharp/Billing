using Billing.Application.Common.Abstractions;
using Billing.Application.Common.Invoices.Command.Create;
using Billing.Application.Common.Invoices.Dto;
using Billing.Domain.Invoices;
using Billing.Infrastructure.Persistence;

using FluentAssertions;

using Microsoft.EntityFrameworkCore;

using Moq;

namespace Billing.Tests.Application;

public class IssueInvoiceHandlerTests
{
    private static BillingWriteContext CreateContext()
    {
        var tenant = new Mock<ITenantProvider>();
        tenant.Setup(t => t.TenantId).Returns(Guid.NewGuid());

        var options = new DbContextOptionsBuilder<BillingWriteContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new BillingWriteContext(options, tenant.Object);
    }

    [Fact]
    public async Task Handle_ValidCommand_ReturnsNonEmptyId()
    {
        await using var ctx = CreateContext();
        var handler = new IssueInvoiceHandler(ctx);

        var cmd = new IssueInvoiceCommand(
            "INV-001", "EUR", [new InvoiceLineDto("Service fee", 100m)]);

        var id = await handler.Handle(cmd, CancellationToken.None);

        id.Should().NotBeEmpty();
    }

    [Fact]
    public async Task Handle_ValidCommand_TrackesInvoiceAsIssued()
    {
        await using var ctx = CreateContext();
        var handler = new IssueInvoiceHandler(ctx);

        var cmd = new IssueInvoiceCommand(
            "INV-001", "EUR",
            [new InvoiceLineDto("Line 1", 200m), new InvoiceLineDto("Line 2", 100m)]);

        var id = await handler.Handle(cmd, CancellationToken.None);

        var tracked = ctx.ChangeTracker.Entries<Invoice>().Single().Entity;
        tracked.Id.Value.Should().Be(id);
        tracked.Status.Should().Be(InvoiceStatus.Issued);
        tracked.Total.Amount.Should().Be(300m);
        tracked.Total.Currency.Should().Be("EUR");
        tracked.Lines.Should().HaveCount(2);
    }

    [Fact]
    public async Task Handle_MultipleLines_TotalIsSum()
    {
        await using var ctx = CreateContext();
        var handler = new IssueInvoiceHandler(ctx);

        var cmd = new IssueInvoiceCommand(
            "INV-002", "USD",
            [
                new InvoiceLineDto("Consulting", 500m),
                new InvoiceLineDto("License", 200m),
                new InvoiceLineDto("Support", 50m)
            ]);

        await handler.Handle(cmd, CancellationToken.None);

        var invoice = ctx.ChangeTracker.Entries<Invoice>().Single().Entity;
        invoice.Total.Amount.Should().Be(750m);
        invoice.Total.Currency.Should().Be("USD");
    }
}
