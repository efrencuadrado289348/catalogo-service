using Catalog.Domain.Entities;
using Catalog.Domain.Exceptions;
using Xunit;

namespace Catalog.UnitTests.Domain;

public class ProductTests
{
    private static Product CreateSampleProduct(int stock = 10) =>
        Product.Create("Mechanical Keyboard", "RGB Gaming Keyboard", 79.99m, "USD", stock, Guid.NewGuid());

    [Fact]
    public void Create_WithValidData_ShouldInitializeCorrectly()
    {
        var categoryId = Guid.NewGuid();
        var product = Product.Create("Wireless Mouse", "Ergonomic Bluetooth Mouse", 49.99m, "USD", 25, categoryId);

        Assert.NotEqual(Guid.Empty, product.Id);
        Assert.Equal("Wireless Mouse", product.Name);
        Assert.Equal("Ergonomic Bluetooth Mouse", product.Description);
        Assert.Equal(49.99m, product.Price.Amount);
        Assert.Equal("USD", product.Price.Currency);
        Assert.Equal(25, product.Stock);
        Assert.True(product.Active);
        Assert.Equal(categoryId, product.CategoryId);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Create_WithInvalidName_ShouldThrowDomainException(string? invalidName)
    {
        var exception = Assert.Throws<DomainException>(() =>
            Product.Create(invalidName!, "Description", 10m, "USD", 5, Guid.NewGuid()));
        Assert.Equal("Product name is required.", exception.Message);
    }

    [Fact]
    public void Create_WithNegativeStock_ShouldThrowDomainException()
    {
        var exception = Assert.Throws<DomainException>(() =>
            Product.Create("Item", "Description", 10m, "USD", -1, Guid.NewGuid()));
        Assert.Equal("Initial stock cannot be negative.", exception.Message);
    }

    [Fact]
    public void Create_WithEmptyCategoryId_ShouldThrowDomainException()
    {
        var exception = Assert.Throws<DomainException>(() =>
            Product.Create("Item", "Description", 10m, "USD", 5, Guid.Empty));
        Assert.Equal("Product must belong to a category.", exception.Message);
    }

    [Fact]
    public void UpdatePrice_ShouldChangePrice()
    {
        var product = CreateSampleProduct();
        product.UpdatePrice(89.99m, "USD");

        Assert.Equal(89.99m, product.Price.Amount);
        Assert.Equal("USD", product.Price.Currency);
    }

    [Fact]
    public void AddStock_ShouldIncreaseStock()
    {
        var product = CreateSampleProduct(10);
        product.AddStock(5);

        Assert.Equal(15, product.Stock);
    }

    [Fact]
    public void DeductStock_ShouldDecreaseStock()
    {
        var product = CreateSampleProduct(10);
        product.DeductStock(4);

        Assert.Equal(6, product.Stock);
    }

    [Fact]
    public void DeductStock_ExceedingAvailableStock_ShouldThrowDomainException()
    {
        var product = CreateSampleProduct(5);
        var exception = Assert.Throws<DomainException>(() => product.DeductStock(10));
        Assert.Contains("Insufficient stock", exception.Message);
    }

    [Fact]
    public void Deactivate_ShouldSetActiveToFalse()
    {
        var product = CreateSampleProduct();
        product.Deactivate();

        Assert.False(product.Active);
    }

    [Fact]
    public void Deactivate_WhenAlreadyDeactivated_ShouldThrowDomainException()
    {
        var product = CreateSampleProduct();
        product.Deactivate();

        var exception = Assert.Throws<DomainException>(() => product.Deactivate());
        Assert.Equal("Product is already deactivated.", exception.Message);
    }

    [Fact]
    public void ModifyingDeactivatedProduct_ShouldThrowDomainException()
    {
        var product = CreateSampleProduct();
        product.Deactivate();

        var exception = Assert.Throws<DomainException>(() => product.AddStock(5));
        Assert.Equal("Cannot modify a deactivated product.", exception.Message);
    }
}
