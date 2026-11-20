using Catalog.Application.Categories.Commands;
using Catalog.Application.Categories.Queries;
using Catalog.Application.Exceptions;
using Catalog.Domain.Entities;
using Catalog.Domain.Exceptions;
using Catalog.UnitTests.Fakes;
using Xunit;

namespace Catalog.UnitTests.Application;

public class CategoryCommandQueryTests
{
    private readonly FakeCategoryRepository _categoryRepo = new();
    private readonly FakeUnitOfWork _unitOfWork = new();

    [Fact]
    public async Task CreateCategory_WithUniqueName_ShouldAddCategoryAndCommit()
    {
        var handler = new CreateCategoryHandler(_categoryRepo, _unitOfWork);
        var command = new CreateCategoryCommand("Monitors", "Display screens");

        var categoryId = await handler.Handle(command, CancellationToken.None);

        Assert.NotEqual(Guid.Empty, categoryId);
        Assert.Single(_categoryRepo.Categories);
        Assert.Equal(1, _unitOfWork.SaveChangesCalls);
    }

    [Fact]
    public async Task CreateCategory_WithDuplicateName_ShouldThrowDomainException()
    {
        var existing = Category.Create("Monitors");
        await _categoryRepo.AddAsync(existing);

        var handler = new CreateCategoryHandler(_categoryRepo, _unitOfWork);
        var command = new CreateCategoryCommand("Monitors");

        await Assert.ThrowsAsync<DomainException>(() => handler.Handle(command, CancellationToken.None));
    }

    [Fact]
    public async Task GetCategoryById_WhenFound_ShouldReturnDto()
    {
        var category = Category.Create("Laptops", "Portable computers");
        await _categoryRepo.AddAsync(category);

        var handler = new GetCategoryByIdHandler(_categoryRepo);
        var result = await handler.Handle(new GetCategoryByIdQuery(category.Id), CancellationToken.None);

        Assert.Equal(category.Id, result.Id);
        Assert.Equal("Laptops", result.Name);
        Assert.Equal("Portable computers", result.Description);
    }

    [Fact]
    public async Task GetCategoryById_WhenNotFound_ShouldThrowNotFoundException()
    {
        var handler = new GetCategoryByIdHandler(_categoryRepo);

        await Assert.ThrowsAsync<NotFoundException>(() =>
            handler.Handle(new GetCategoryByIdQuery(Guid.NewGuid()), CancellationToken.None));
    }

    [Fact]
    public async Task ListCategories_ShouldReturnAllCategories()
    {
        await _categoryRepo.AddAsync(Category.Create("Audio"));
        await _categoryRepo.AddAsync(Category.Create("Video"));

        var handler = new ListCategoriesHandler(_categoryRepo);
        var result = await handler.Handle(new ListCategoriesQuery(), CancellationToken.None);

        Assert.Equal(2, result.Count);
    }
}
