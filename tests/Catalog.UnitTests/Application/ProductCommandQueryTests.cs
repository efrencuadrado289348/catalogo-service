using Catalog.Application.Exceptions;
using Catalog.Application.Products.Commands;
using Catalog.Application.Products.Queries;
using Catalog.Domain.Entities;
using Catalog.Domain.Exceptions;
using Catalog.UnitTests.Fakes;
using Xunit;

namespace Catalog.UnitTests.Application;

public class ProductCommandQueryTests
{
    private readonly FakeProductRepository _productRepo = new();
    private readonly FakeCategoryRepository _categoryRepo = new();
    private readonly FakeUnitOfWork _unitOfWork = new();

    private async Task<Category> SetupCategoryAsync(string name = "Electronics")
    {
        var category = Category.Create(name);
        await _categoryRepo.AddAsync(category);
        return category;
    }

    [Fact]
    public async Task CreateProduct_WithValidData_ShouldAddProductAndCommit()
    {
        var category = await SetupCategoryAsync();
        var handler = new CreateProductHandler(_productRepo, _categoryRepo, _unitOfWork);
        var command = new CreateProductCommand(
            "Noise Canceling Headphones",
            "Wireless over-ear headphones",
            299.99m,
            "USD",
            15,
            category.Id);

        var productId = await handler.Handle(command, CancellationToken.None);

        Assert.NotEqual(Guid.Empty, productId);
        Assert.Single(_productRepo.Products);
        Assert.Equal(1, _unitOfWork.SaveChangesCalls);
    }

    [Fact]
    public async Task CreateProduct_WithNonExistentCategory_ShouldThrowDomainException()
    {
        var handler = new CreateProductHandler(_productRepo, _categoryRepo, _unitOfWork);
        var command = new CreateProductCommand("Gadget", null, 19.99m, "USD", 5, Guid.NewGuid());

        await Assert.ThrowsAsync<DomainException>(() => handler.Handle(command, CancellationToken.None));
    }

    [Fact]
    public async Task UpdatePrice_ShouldUpdateProductPriceAndCommit()
    {
        var category = await SetupCategoryAsync();
        var product = Product.Create("Smart TV", "4K OLED", 1200m, "USD", 5, category.Id);
        await _productRepo.AddAsync(product);

        var handler = new UpdatePriceHandler(_productRepo, _unitOfWork);
        await handler.Handle(new UpdatePriceCommand(product.Id, 1099.99m, "USD"), CancellationToken.None);

        Assert.Equal(1099.99m, product.Price.Amount);
        Assert.Equal(1, _unitOfWork.SaveChangesCalls);
    }

    [Fact]
    public async Task AdjustStock_Positive_ShouldIncreaseStock()
    {
        var category = await SetupCategoryAsync();
        var product = Product.Create("Smart TV", "4K OLED", 1200m, "USD", 5, category.Id);
        await _productRepo.AddAsync(product);

        var handler = new AdjustStockHandler(_productRepo, _unitOfWork);
        await handler.Handle(new AdjustStockCommand(product.Id, 10), CancellationToken.None);

        Assert.Equal(15, product.Stock);
        Assert.Equal(1, _unitOfWork.SaveChangesCalls);
    }

    [Fact]
    public async Task AdjustStock_Negative_ShouldDecreaseStock()
    {
        var category = await SetupCategoryAsync();
        var product = Product.Create("Smart TV", "4K OLED", 1200m, "USD", 5, category.Id);
        await _productRepo.AddAsync(product);

        var handler = new AdjustStockHandler(_productRepo, _unitOfWork);
        await handler.Handle(new AdjustStockCommand(product.Id, -2), CancellationToken.None);

        Assert.Equal(3, product.Stock);
        Assert.Equal(1, _unitOfWork.SaveChangesCalls);
    }

    [Fact]
    public async Task DeactivateProduct_ShouldDeactivateAndCommit()
    {
        var category = await SetupCategoryAsync();
        var product = Product.Create("Smart TV", "4K OLED", 1200m, "USD", 5, category.Id);
        await _productRepo.AddAsync(product);

        var handler = new DeactivateProductHandler(_productRepo, _unitOfWork);
        await handler.Handle(new DeactivateProductCommand(product.Id), CancellationToken.None);

        Assert.False(product.Active);
        Assert.Equal(1, _unitOfWork.SaveChangesCalls);
    }

    [Fact]
    public async Task GetProductById_WhenFound_ShouldReturnProductDto()
    {
        var category = await SetupCategoryAsync();
        var product = Product.Create("Smart TV", "4K OLED", 1200m, "USD", 5, category.Id);
        await _productRepo.AddAsync(product);

        var handler = new GetProductByIdHandler(_productRepo);
        var result = await handler.Handle(new GetProductByIdQuery(product.Id), CancellationToken.None);

        Assert.Equal(product.Id, result.Id);
        Assert.Equal("Smart TV", result.Name);
        Assert.Equal(1200m, result.Price);
    }

    [Fact]
    public async Task GetProductById_WhenNotFound_ShouldThrowNotFoundException()
    {
        var handler = new GetProductByIdHandler(_productRepo);

        await Assert.ThrowsAsync<NotFoundException>(() =>
            handler.Handle(new GetProductByIdQuery(Guid.NewGuid()), CancellationToken.None));
    }
}
