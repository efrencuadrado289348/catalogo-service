
namespace Pedidos.Application.Interfaces;

public interface IProductCatalogService
{
    Task<CatalogProduct?> GetProductByIdAsync(
        Guid productId,
        CancellationToken cancellationToken = default);
}

public record CatalogProduct(
    Guid Id,
    string Name,
    decimal Price
);
