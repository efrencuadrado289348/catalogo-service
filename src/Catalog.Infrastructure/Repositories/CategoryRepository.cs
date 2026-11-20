using Catalog.Domain.Entities;
using Catalog.Domain.Repositories;
using Catalog.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Catalog.Infrastructure.Repositories;

public class CategoryRepository : ICategoryRepository
{
    private readonly CatalogDbContext _context;

    public CategoryRepository(CatalogDbContext context)
    {
        _context = context;
    }

    public async Task<Category?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await _context.Categories
            .FirstOrDefaultAsync(c => c.Id == id, ct);
    }

    public async Task<IReadOnlyList<Category>> ListAsync(CancellationToken ct = default)
    {
        return await _context.Categories
            .AsNoTracking()
            .ToListAsync(ct);
    }

    public async Task<bool> ExistsAsync(Guid id, CancellationToken ct = default)
    {
        return await _context.Categories
            .AsNoTracking()
            .AnyAsync(c => c.Id == id, ct);
    }

    public async Task<bool> ExistsByNameAsync(string name, CancellationToken ct = default)
    {
        return await _context.Categories
            .AsNoTracking()
            .AnyAsync(c => c.Name == name, ct);
    }

    public async Task AddAsync(Category category, CancellationToken ct = default)
    {
        await _context.Categories.AddAsync(category, ct);
    }
}