
using System.Net;
using System.Net.Http.Json;
using Pedidos.Application.Interfaces;

namespace Pedidos.Infrastructure.ExternalServices;

public class ProductCatalogService(HttpClient httpClient)
    : IProductCatalogService
{
    public async Task<CatalogProduct?> GetProductByIdAsync(
        Guid productId,
        CancellationToken cancellationToken = default)
    {
        var response = await httpClient.GetAsync(
            $"api/products/{productId}",
            cancellationToken);

        if (response.StatusCode == HttpStatusCode.NotFound)
            return null;

        response.EnsureSuccessStatusCode();

        var product = await response.Content.ReadFromJsonAsync<CatalogProduct>(
            cancellationToken);

        if (product is null)
            throw new InvalidOperationException(
                "Catálogo devolvió una respuesta vacía.");

        return product;
    }
}
