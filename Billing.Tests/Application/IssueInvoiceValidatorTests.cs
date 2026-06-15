using Billing.Application.Common.Invoices.Command.Create;
using Billing.Application.Common.Invoices.Dto;

using FluentAssertions;

namespace Billing.Tests.Application;

public class IssueInvoiceValidatorTests
{
    private readonly IssueInvoiceValidator _sut = new();

    private static IssueInvoiceCommand ValidCommand() =>
        new("INV-001", "EUR", [new InvoiceLineDto("Service fee", 100m)]);

    [Fact]
    public void Validate_ValidCommand_Passes()
    {
        var result = _sut.Validate(ValidCommand());

        result.IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public void Validate_EmptyNumber_Fails(string? number)
    {
        var cmd = ValidCommand() with { Number = number! };

        var result = _sut.Validate(cmd);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(IssueInvoiceCommand.Number));
    }

    [Fact]
    public void Validate_NumberTooLong_Fails()
    {
        var cmd = ValidCommand() with { Number = new string('X', 41) };

        var result = _sut.Validate(cmd);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(IssueInvoiceCommand.Number));
    }

    [Theory]
    [InlineData("")]
    [InlineData("EU")]
    [InlineData("EURO")]
    public void Validate_InvalidCurrency_Fails(string currency)
    {
        var cmd = ValidCommand() with { Currency = currency };

        var result = _sut.Validate(cmd);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(IssueInvoiceCommand.Currency));
    }

    [Fact]
    public void Validate_EmptyLines_Fails()
    {
        var cmd = ValidCommand() with { Lines = [] };

        var result = _sut.Validate(cmd);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(IssueInvoiceCommand.Lines));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Validate_LineAmountNotPositive_Fails(decimal amount)
    {
        var cmd = ValidCommand() with { Lines = [new InvoiceLineDto("Service", amount)] };

        var result = _sut.Validate(cmd);

        result.IsValid.Should().BeFalse();
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public void Validate_EmptyLineDescription_Fails(string? description)
    {
        var cmd = ValidCommand() with { Lines = [new InvoiceLineDto(description!, 100m)] };

        var result = _sut.Validate(cmd);

        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public void Validate_LineDescriptionTooLong_Fails()
    {
        var cmd = ValidCommand() with
        {
            Lines = [new InvoiceLineDto(new string('A', 201), 100m)]
        };

        var result = _sut.Validate(cmd);

        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public void Validate_MaxLengthNumber_Passes()
    {
        var cmd = ValidCommand() with { Number = new string('X', 40) };

        var result = _sut.Validate(cmd);

        result.IsValid.Should().BeTrue();
    }
}
