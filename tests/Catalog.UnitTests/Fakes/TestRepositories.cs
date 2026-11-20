using Catalog.Application.Abstractions;
using Catalog.Domain.Entities;
using Catalog.Domain.Repositories;

namespace Catalog.UnitTests.Fakes;

public class FakeCategoryRepository : ICategoryRepository
{
    public readonly List<Category> Categories = [];

    public Task<Category?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        Task.FromResult(Categories.FirstOrDefault(c => c.Id == id));

    public Task<IReadOnlyList<Category>> ListAsync(CancellationToken ct = default) =>
        Task.FromResult<IReadOnlyList<Category>>(Categories);

    public Task<bool> ExistsAsync(Guid id, CancellationToken ct = default) =>
        Task.FromResult(Categories.Any(c => c.Id == id));

    public Task<bool> ExistsByNameAsync(string name, CancellationToken ct = default) =>
        Task.FromResult(Categories.Any(c => string.Equals(c.Name, name, StringComparison.OrdinalIgnoreCase)));

    public Task AddAsync(Category category, CancellationToken ct = default)
    {
        Categories.Add(category);
        return Task.CompletedTask;
    }
}

public class FakeProductRepository : IProductRepository
{
    public readonly List<Product> Products = [];

    public Task<Product?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        Task.FromResult(Products.FirstOrDefault(p => p.Id == id));

    public Task<IReadOnlyList<Product>> ListAsync(Guid? categoryId = null, CancellationToken ct = default)
    {
        var result = categoryId.HasValue
            ? Products.Where(p => p.CategoryId == categoryId.Value).ToList()
            : Products;
        return Task.FromResult<IReadOnlyList<Product>>(result);
    }

    public Task<bool> ExistsByNameAsync(string name, CancellationToken ct = default) =>
        Task.FromResult(Products.Any(p => string.Equals(p.Name, name, StringComparison.OrdinalIgnoreCase)));

    public Task AddAsync(Product product, CancellationToken ct = default)
    {
        Products.Add(product);
        return Task.CompletedTask;
    }
}

public class FakeUnitOfWork : IUnitOfWork
{
    public int SaveChangesCalls { get; private set; }

    public Task<int> SaveChangesAsync(CancellationToken ct = default)
    {
        SaveChangesCalls++;
        return Task.FromResult(1);
    }
}
