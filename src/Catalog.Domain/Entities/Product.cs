using Catalog.Domain.Exceptions;
using Catalog.Domain.ValueObjects;

namespace Catalog.Domain.Entities;

/// <summary>
/// Aggregate root: all state changes go through its methods,
/// which validate business rules. Setters are private.
/// </summary>
public class Product
{
    public Guid Id { get; private set; }
    public string Name { get; private set; }
    public string? Description { get; private set; }
    public Price Price { get; private set; }
    public int Stock { get; private set; }
    public bool Active { get; private set; }
    public DateTime CreatedAt { get; private set; }

    // Only CategoryId: Category is another aggregate, the entity is not referenced directly.
    public Guid CategoryId { get; private set; }

    // Private parameterless constructor for EF Core
    private Product()
    {
        Name = string.Empty;
        Price = null!;
    }

    private Product(Guid id, string name, string? description, Price price, int stock, Guid categoryId)
    {
        Id = id;
        Name = name;
        Description = description;
        Price = price;
        Stock = stock;
        CategoryId = categoryId;
        Active = true;
        CreatedAt = DateTime.UtcNow;
    }

    public static Product Create(
        string name,
        string? description,
        decimal price,
        string? currency,
        int stock,
        Guid categoryId)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("Product name is required.");

        if (name.Trim().Length > 150)
            throw new DomainException("Product name cannot exceed 150 characters.");

        if (stock < 0)
            throw new DomainException("Initial stock cannot be negative.");

        if (categoryId == Guid.Empty)
            throw new DomainException("Product must belong to a category.");

        return new Product(
            Guid.NewGuid(),
            name.Trim(),
            description?.Trim(),
            Price.Create(price, currency),
            stock,
            categoryId);
    }

    public void UpdatePrice(decimal newAmount, string? currency = null)
    {
        EnsureActive();
        Price = Price.Create(newAmount, currency ?? Price.Currency);
    }

    public void AddStock(int quantity)
    {
        EnsureActive();

        if (quantity <= 0)
            throw new DomainException("Quantity to add must be greater than zero.");

        Stock += quantity;
    }

    public void DeductStock(int quantity)
    {
        EnsureActive();

        if (quantity <= 0)
            throw new DomainException("Quantity to deduct must be greater than zero.");

        if (quantity > Stock)
            throw new DomainException($"Insufficient stock. Available: {Stock}, requested: {quantity}.");

        Stock -= quantity;
    }

    public void Deactivate()
    {
        if (!Active)
            throw new DomainException("Product is already deactivated.");

        Active = false;
    }

    private void EnsureActive()
    {
        if (!Active)
            throw new DomainException("Cannot modify a deactivated product.");
    }
}
