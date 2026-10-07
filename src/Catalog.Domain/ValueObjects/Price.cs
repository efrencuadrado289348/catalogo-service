using Catalog.Domain.Exceptions;

namespace Catalog.Domain.ValueObjects;

/// <summary>
/// Value object: has no identity, is immutable, and is compared by value.
/// If values are invalid, the object cannot be created.
/// </summary>
public sealed record Price
{
    public const string DefaultCurrency = "USD";

    public decimal Amount { get; private set; }
    public string Currency { get; private set; }

    // Private parameterless constructor required by EF Core for materialization
    private Price()
    {
        Currency = DefaultCurrency;
    }

    private Price(decimal amount, string currency)
    {
        Amount = amount;
        Currency = currency;
    }

    public static Price Create(decimal amount, string? currency = null)
    {
        if (amount <= 0)
            throw new DomainException("Price must be greater than zero.");

        var normalizedCurrency = string.IsNullOrWhiteSpace(currency)
            ? DefaultCurrency
            : currency.Trim().ToUpperInvariant();

        if (normalizedCurrency.Length != 3)
            throw new DomainException("Currency must be a 3-letter code (e.g., USD or COP).");

        return new Price(amount, normalizedCurrency);
    }
}
