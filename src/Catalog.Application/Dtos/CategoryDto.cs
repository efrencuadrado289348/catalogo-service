using Catalog.Domain.Entities;

namespace Catalog.Application.Dtos;

public sealed record CategoryDto(Guid Id, string Name, string? Description)
{
    public static CategoryDto From(Category c) => new(c.Id, c.Name, c.Description);
}
