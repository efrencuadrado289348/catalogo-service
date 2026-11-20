using Catalog.Domain.Entities;
using Catalog.Domain.Repositories;
using Catalog.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Catalog.Infrastructure.Repositories;

public class ProductRepository : IProductRepository
{
    private readonly CatalogDbContext _context;

    public ProductRepository(CatalogDbContext context)
    {
        _context = context;
    }

    public async Task<Product?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await _context.Products
            .FirstOrDefaultAsync(p => p.Id == id, ct);
    }

    public async Task<IReadOnlyList<Product>> ListAsync(
        Guid? categoryId = null,
        CancellationToken ct = default)
    {
        var query = _context.Products
            .AsNoTracking()
            .AsQueryable();

        if (categoryId.HasValue)
        {
            query = query.Where(p => p.CategoryId == categoryId.Value);
        }

        return await query.ToListAsync(ct);
    }

    public async Task<bool> ExistsByNameAsync(string name, CancellationToken ct = default)
    {
        return await _context.Products
            .AsNoTracking()
            .AnyAsync(p => p.Name == name, ct);
    }

    public async Task AddAsync(Product product, CancellationToken ct = default)
    {
        await _context.Products.AddAsync(product, ct);
    }
}