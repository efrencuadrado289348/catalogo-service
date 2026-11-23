namespace Pedidos.Domain.Entities;

public class OrderItem
{
    public Guid Id { get; private set; }
    public Guid ProductId { get; private set; }
    public string ProductName { get; private set; } = string.Empty;
    public int Quantity { get; private set; }
    public decimal UnitPrice { get; private set; }

    public decimal Subtotal => Quantity * UnitPrice;

    private OrderItem()
    {
    }

    private OrderItem(
        Guid productId,
        string productName,
        int quantity,
        decimal unitPrice)
    {
        Id = Guid.NewGuid();
        ProductId = productId;
        ProductName = productName;
        Quantity = quantity;
        UnitPrice = unitPrice;
    }

    public static OrderItem Create(
        Guid productId,
        string productName,
        int quantity,
        decimal unitPrice)
    {
        if (productId == Guid.Empty)
            throw new ArgumentException("El producto es obligatorio.");

        if (string.IsNullOrWhiteSpace(productName))
            throw new ArgumentException("El nombre del producto es obligatorio.");

        if (quantity <= 0)
            throw new ArgumentException("La cantidad debe ser mayor que cero.");

        if (unitPrice <= 0)
            throw new ArgumentException("El precio debe ser mayor que cero.");

        return new OrderItem(productId, productName, quantity, unitPrice);
    }
}