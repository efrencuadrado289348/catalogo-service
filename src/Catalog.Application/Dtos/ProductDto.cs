using Catalog.Domain.Entities;

namespace Catalog.Application.Dtos;

public sealed record ProductDto(
    Guid Id,
    string Name,
    string? Description,
    decimal Price,
    string Currency,
    int Stock,
    Guid CategoryId,
    bool Active)
{
    public static ProductDto From(Product p) =>
        new(p.Id, p.Name, p.Description, p.Price.Amount, p.Price.Currency, p.Stock, p.CategoryId, p.Active);
}
