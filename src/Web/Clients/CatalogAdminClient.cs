using System.Net;
using Microsoft.AspNetCore.Mvc;

namespace Web.Clients;

// Nombres propios (Admin*) para no chocar con los records de Persona 2.
public sealed record AdminProductRow(Guid Id, string Name, string? Description,
    decimal Price, string Currency, int Stock, Guid CategoryId, bool Active);
public sealed record AdminCategoryRow(Guid Id, string Name);

public sealed record CreateCategoryRequest(string Name, string? Description);
public sealed record CreateProductRequest(string Name, string? Description,
    decimal Price, string Currency, int Stock, Guid CategoryId);
public sealed record ChangePriceRequest(decimal Amount, string Currency);
public sealed record ChangeStockRequest(int Quantity);

public sealed class CatalogAdminClient(HttpClient http)
{
    public async Task<IReadOnlyList<AdminProductRow>> GetProductsAsync(CancellationToken ct)
    {
        using var response = await http.GetAsync("api/products", ct);
        await EnsureSuccessAsync(response, ct);
        return await response.Content.ReadFromJsonAsync<List<AdminProductRow>>(ct) ?? [];
    }

    public async Task<IReadOnlyList<AdminCategoryRow>> GetCategoriesAsync(CancellationToken ct)
    {
        using var response = await http.GetAsync("api/categories", ct);
        await EnsureSuccessAsync(response, ct);
        return await response.Content.ReadFromJsonAsync<List<AdminCategoryRow>>(ct) ?? [];
    }

    public async Task CreateCategoryAsync(CreateCategoryRequest body, CancellationToken ct)
    {
        using var response = await http.PostAsJsonAsync("api/categories", body, ct);
        await EnsureSuccessAsync(response, ct);
    }

    public async Task CreateProductAsync(CreateProductRequest body, CancellationToken ct)
    {
        using var response = await http.PostAsJsonAsync("api/products", body, ct);
        await EnsureSuccessAsync(response, ct);
    }

    public async Task ChangePriceAsync(Guid id, decimal amount, string currency, CancellationToken ct)
    {
        using var response = await http.PatchAsJsonAsync($"api/products/{id}/price",
            new ChangePriceRequest(amount, currency), ct);
        await EnsureSuccessAsync(response, ct);
    }

    public async Task ChangeStockAsync(Guid id, int quantity, CancellationToken ct)
    {
        using var response = await http.PatchAsJsonAsync($"api/products/{id}/stock", new ChangeStockRequest(quantity), ct);
        await EnsureSuccessAsync(response, ct);
    }

    public async Task DeactivateAsync(Guid id, CancellationToken ct)
    {
        using var response = await http.PatchAsync($"api/products/{id}/deactivate", null, ct);
        await EnsureSuccessAsync(response, ct);
    }

    // 400/409/422: reglas de negocio de la API -> mensaje legible.
    // 401/403: los maneja la base (cierra sesión o "Acceso denegado").
    private static async Task EnsureSuccessAsync(HttpResponseMessage response, CancellationToken ct)
    {
        if (response.StatusCode == HttpStatusCode.NotFound)
            throw new InvalidOperationException("El recurso solicitado no existe.");

        if (response.StatusCode is HttpStatusCode.BadRequest or HttpStatusCode.Conflict
            or HttpStatusCode.UnprocessableEntity)
        {
            var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>(ct);
            throw new InvalidOperationException(problem?.Detail ?? problem?.Title ?? "Solicitud inválida.");
        }

        response.EnsureSuccessStatusCode();
    }
}
