using Catalogo.Domain.Entities;
using Catalogo.Domain.Repositories;
using Catalogo.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Catalogo.Infrastructure.Repositories;

public class ProductoRepository : IProductoRepository
{
    private readonly CatalogoDbContext _context;

    public ProductoRepository(CatalogoDbContext context)
    {
        _context = context;
    }

    public async Task<Producto?> ObtenerPorIdAsync(
        Guid id,
        CancellationToken ct = default)
    {
        return await _context.Productos
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == id, ct);
    }

    public async Task<IReadOnlyList<Producto>> ListarAsync(
        Guid? categoriaId = null,
        CancellationToken ct = default)
    {
        var query = _context.Productos
            .AsNoTracking()
            .AsQueryable();

        if (categoriaId.HasValue)
        {
            query = query.Where(p => p.CategoriaId == categoriaId.Value);
        }

        return await query.ToListAsync(ct);
    }

    public async Task<bool> ExisteNombreAsync(
        string nombre,
        CancellationToken ct = default)
    {
        return await _context.Productos
            .AsNoTracking()
            .AnyAsync(p => p.Nombre == nombre, ct);
    }

    public async Task AgregarAsync(
        Producto producto,
        CancellationToken ct = default)
    {
        await _context.Productos.AddAsync(producto, ct);
    }
}