using Catalogo.Domain.Entities;

namespace Catalogo.Domain.Repositories;

public interface ICategoriaRepository
{
    Task<Categoria?> ObtenerPorIdAsync(Guid id, CancellationToken ct = default);
    Task<IReadOnlyList<Categoria>> ListarAsync(CancellationToken ct = default);
    Task<bool> ExisteAsync(Guid id, CancellationToken ct = default);
    Task<bool> ExisteNombreAsync(string nombre, CancellationToken ct = default);
    Task AgregarAsync(Categoria categoria, CancellationToken ct = default);
}
