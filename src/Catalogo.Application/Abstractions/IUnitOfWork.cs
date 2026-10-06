namespace Catalogo.Application.Abstractions;

/// Confirma en una sola transacción todos los cambios hechos por los repositorios.
public interface IUnitOfWork
{
    Task<int> SaveChangesAsync(CancellationToken ct = default);
}
