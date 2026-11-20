using Catalog.Domain.Entities;

namespace Catalog.Domain.Repositories;

public interface IProductRepository
{
    Task<Product?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<IReadOnlyList<Product>> ListAsync(Guid? categoryId = null, CancellationToken ct = default);
    Task<bool> ExistsByNameAsync(string name, CancellationToken ct = default);
    Task AddAsync(Product product, CancellationToken ct = default);
}
