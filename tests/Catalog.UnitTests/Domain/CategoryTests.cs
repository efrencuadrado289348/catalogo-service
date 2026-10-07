using Catalog.Domain.Entities;
using Catalog.Domain.Exceptions;
using Xunit;

namespace Catalog.UnitTests.Domain;

public class CategoryTests
{
    [Fact]
    public void Create_WithValidData_ShouldCreateCategory()
    {
        var category = Category.Create("Smartphones", "Mobile devices and accessories");

        Assert.NotEqual(Guid.Empty, category.Id);
        Assert.Equal("Smartphones", category.Name);
        Assert.Equal("Mobile devices and accessories", category.Description);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Create_WithEmptyName_ShouldThrowDomainException(string? invalidName)
    {
        var exception = Assert.Throws<DomainException>(() => Category.Create(invalidName!));
        Assert.Equal("Category name is required.", exception.Message);
    }

    [Fact]
    public void Create_WithNameExceeding100Chars_ShouldThrowDomainException()
    {
        var longName = new string('A', 101);
        var exception = Assert.Throws<DomainException>(() => Category.Create(longName));
        Assert.Equal("Category name cannot exceed 100 characters.", exception.Message);
    }
}
