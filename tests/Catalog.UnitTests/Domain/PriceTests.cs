using Catalog.Domain.Exceptions;
using Catalog.Domain.ValueObjects;
using Xunit;

namespace Catalog.UnitTests.Domain;

public class PriceTests
{
    [Fact]
    public void Create_WithValidAmountAndDefaultCurrency_ShouldUseDefaultCurrency()
    {
        var price = Price.Create(99.99m);

        Assert.Equal(99.99m, price.Amount);
        Assert.Equal("USD", price.Currency);
    }

    [Fact]
    public void Create_WithCustomCurrency_ShouldNormalizeCurrency()
    {
        var price = Price.Create(150.00m, "cop");

        Assert.Equal(150.00m, price.Amount);
        Assert.Equal("COP", price.Currency);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-10)]
    public void Create_WithInvalidAmount_ShouldThrowDomainException(decimal invalidAmount)
    {
        var exception = Assert.Throws<DomainException>(() => Price.Create(invalidAmount));
        Assert.Equal("Price must be greater than zero.", exception.Message);
    }

    [Theory]
    [InlineData("US")]
    [InlineData("USDT")]
    public void Create_WithInvalidCurrencyLength_ShouldThrowDomainException(string invalidCurrency)
    {
        var exception = Assert.Throws<DomainException>(() => Price.Create(50, invalidCurrency));
        Assert.Equal("Currency must be a 3-letter code (e.g., USD or COP).", exception.Message);
    }
}
