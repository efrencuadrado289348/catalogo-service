
using System.Net.Http.Json;

namespace Web.Clients;

public sealed record ProductItem(
    Guid Id,
    string Name,
    string? Description,
    decimal Price,
    string Currency,
    int Stock,
    Guid CategoryId,
    bool Active);

public sealed record CategoryItem(
    Guid Id,
    string Name,
    string? Description);

public sealed class CatalogClient(HttpClient http)
{
    public async Task<IReadOnlyList<ProductItem>> GetProductsAsync(
        Guid? categoryId,
        CancellationToken ct)
    {
        var url = categoryId is null
            ? "api/products"
            : $"api/products?categoryId={categoryId}";

        using var response = await http.GetAsync(url, ct);
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<List<ProductItem>>(ct) ?? [];
    }

    public async Task<ProductItem?> GetProductByIdAsync(
        Guid id,
        CancellationToken ct)
    {
        using var response = await http.GetAsync($"api/products/{id}", ct);
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<ProductItem>(ct);
    }

    public async Task<IReadOnlyList<CategoryItem>> GetCategoriesAsync(
        CancellationToken ct)
    {
        using var response = await http.GetAsync("api/categories", ct);
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<List<CategoryItem>>(ct) ?? [];
    }
}
