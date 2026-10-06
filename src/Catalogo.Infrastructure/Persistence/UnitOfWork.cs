using Catalogo.Application.Abstractions;

namespace Catalogo.Infrastructure.Persistence;

public sealed class UnitOfWork : IUnitOfWork
{
    private readonly CatalogoDbContext _context;

    public UnitOfWork(CatalogoDbContext context)
    {
        _context = context;
    }

    public Task<int> SaveChangesAsync(CancellationToken ct = default)
    {
        return _context.SaveChangesAsync(ct);
    }
}