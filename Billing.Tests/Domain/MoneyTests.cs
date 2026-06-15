using Billing.Domain.Common;
using Billing.Domain.Invoices;

using FluentAssertions;

namespace Billing.Tests.Domain;

public class MoneyTests
{
    [Fact]
    public void Zero_ReturnsZeroAmount()
    {
        var money = Money.Zero("EUR");

        money.Amount.Should().Be(0);
        money.Currency.Should().Be("EUR");
    }

    [Fact]
    public void Constructor_UppercasesCurrency()
    {
        var money = new Money(100m, "eur");

        money.Currency.Should().Be("EUR");
    }

    [Fact]
    public void Constructor_NegativeAmount_ThrowsDomainException()
    {
        var act = () => new Money(-0.01m, "EUR");

        act.Should().ThrowExactly<DomainException>().WithMessage("*negative*");
    }

    [Theory]
    [InlineData("EU")]
    [InlineData("EURO")]
    [InlineData("")]
    public void Constructor_InvalidCurrencyLength_ThrowsDomainException(string currency)
    {
        var act = () => new Money(100m, currency);

        act.Should().ThrowExactly<DomainException>().WithMessage("*3-letter ISO*");
    }

    [Fact]
    public void Add_SameCurrency_ReturnsSummedAmount()
    {
        var a = new Money(100m, "EUR");
        var b = new Money(50.50m, "EUR");

        var result = a.Add(b);

        result.Amount.Should().Be(150.50m);
        result.Currency.Should().Be("EUR");
    }

    [Fact]
    public void Add_DifferentCurrencies_ThrowsDomainException()
    {
        var eur = new Money(100m, "EUR");
        var usd = new Money(50m, "USD");

        var act = () => eur.Add(usd);

        act.Should().ThrowExactly<DomainException>().WithMessage("*currencies*");
    }

    [Fact]
    public void OperatorPlus_SameCurrency_ReturnsSum()
    {
        var a = new Money(100m, "EUR");
        var b = new Money(75m, "EUR");

        var result = a + b;

        result.Amount.Should().Be(175m);
        result.Currency.Should().Be("EUR");
    }

    [Fact]
    public void Constructor_ZeroAmount_IsAllowed()
    {
        var act = () => new Money(0m, "EUR");

        act.Should().NotThrow();
    }
}
