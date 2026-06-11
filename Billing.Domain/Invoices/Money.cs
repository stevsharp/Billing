using Billing.Domain.Common;

namespace Billing.Domain.Invoices;

public sealed record Money
{
    public decimal Amount { get; init; }
    public string Currency { get; init; }

    public Money(decimal amount, string currency)
    {
        if (amount < 0) 
            throw new DomainException("Amount cannot be negative.");

        if (string.IsNullOrWhiteSpace(currency) || currency.Length != 3)
            throw new DomainException("Currency must be a 3-letter ISO code.");

        Amount = amount;
        Currency = currency;

        Amount = amount;
        Currency = currency.ToUpperInvariant();
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="other"></param>
    /// <returns></returns>
    /// <exception cref="DomainException"></exception>
    public Money Add(Money other)
    {
        if (Currency != other.Currency)
            throw new DomainException("Cannot add amounts with different currencies.");

        return new Money(Amount + other.Amount, Currency);
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="currency"></param>
    /// <returns></returns>
    public static Money Zero(string currency) => new Money(0, currency);

    public static Money operator +(Money a, Money b) => a.Add(b);



}