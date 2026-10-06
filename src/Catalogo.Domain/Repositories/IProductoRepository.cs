using Catalogo.Domain.Entities;

namespace Catalogo.Domain.Repositories;

public interface IProductoRepository
{
    Task<Producto?> ObtenerPorIdAsync(Guid id, CancellationToken ct = default);
    Task<IReadOnlyList<Producto>> ListarAsync(Guid? categoriaId = null, CancellationToken ct = default);
    Task<bool> ExisteNombreAsync(string nombre, CancellationToken ct = default);
    Task AgregarAsync(Producto producto, CancellationToken ct = default);
}
