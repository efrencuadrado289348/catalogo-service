using Catalogo.Domain.Entities;
using Catalogo.Domain.Repositories;
using Catalogo.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Catalogo.Infrastructure.Repositories;

public class CategoriaRepository : ICategoriaRepository
{
    private readonly CatalogoDbContext _context;

    public CategoriaRepository(CatalogoDbContext context)
    {
        _context = context;
    }

    public async Task<Categoria?> ObtenerPorIdAsync(
        Guid id,
        CancellationToken ct = default)
    {
        return await _context.Categorias
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == id, ct);
    }

    public async Task<IReadOnlyList<Categoria>> ListarAsync(
        CancellationToken ct = default)
    {
        return await _context.Categorias
            .AsNoTracking()
            .ToListAsync(ct);
    }

    public async Task<bool> ExisteAsync(
        Guid id,
        CancellationToken ct = default)
    {
        return await _context.Categorias
            .AsNoTracking()
            .AnyAsync(c => c.Id == id, ct);
    }

    public async Task<bool> ExisteNombreAsync(
        string nombre,
        CancellationToken ct = default)
    {
        return await _context.Categorias
            .AsNoTracking()
            .AnyAsync(c => c.Nombre == nombre, ct);
    }

    public async Task AgregarAsync(
        Categoria categoria,
        CancellationToken ct = default)
    {
        await _context.Categorias.AddAsync(categoria, ct);
    }
}