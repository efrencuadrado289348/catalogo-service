namespace Catalog.Application.Abstractions;

/// <summary>
/// Commits in a single transaction all changes made by repositories.
/// Repositories add/modify; only the Unit of Work saves changes.
/// </summary>
public interface IUnitOfWork
{
    Task<int> SaveChangesAsync(CancellationToken ct = default);
}
