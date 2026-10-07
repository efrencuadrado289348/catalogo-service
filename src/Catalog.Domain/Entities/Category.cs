using Catalog.Domain.Exceptions;

namespace Catalog.Domain.Entities;

public class Category
{
    public Guid Id { get; private set; }
    public string Name { get; private set; }
    public string? Description { get; private set; }

    // Private parameterless constructor for EF Core
    private Category()
    {
        Name = string.Empty;
    }

    private Category(Guid id, string name, string? description)
    {
        Id = id;
        Name = name;
        Description = description;
    }

    public static Category Create(string name, string? description = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("Category name is required.");

        if (name.Trim().Length > 100)
            throw new DomainException("Category name cannot exceed 100 characters.");

        return new Category(Guid.NewGuid(), name.Trim(), description?.Trim());
    }
}
