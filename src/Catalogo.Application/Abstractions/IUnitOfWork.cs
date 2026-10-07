namespace Catalogo.Application.Abstractions;

/// <summary>
/// Confirma en una sola transacción todos los cambios hechos por los repositorios.
/// Los repositorios agregan/modifican; solo el Unit of Work guarda.
/// </summary>
public interface IUnitOfWork
{
    Task<int> SaveChangesAsync(CancellationToken ct);
}
