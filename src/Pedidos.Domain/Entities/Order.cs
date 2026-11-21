using Pedidos.Domain.Enums;

namespace Pedidos.Domain.Entities;

public class Order
{
    private readonly List<OrderItem> _items = [];

    public Guid Id { get; private set; }
    public Guid CustomerId { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }
    public OrderStatus Status { get; private set; }

    public IReadOnlyCollection<OrderItem> Items => _items.AsReadOnly();

    public decimal Total => _items.Sum(item => item.Subtotal);

    private Order()
    {
    }

    private Order(Guid customerId, IEnumerable<OrderItem> items)
    {
        Id = Guid.NewGuid();
        CustomerId = customerId;
        CreatedAtUtc = DateTime.UtcNow;
        Status = OrderStatus.Pending;
        _items.AddRange(items);
    }

    public static Order Create(Guid customerId, IEnumerable<OrderItem> items)
    {
        if (customerId == Guid.Empty)
            throw new ArgumentException("El cliente es obligatorio.");

        ArgumentNullException.ThrowIfNull(items);

        var orderItems = items.ToList();

        if (orderItems.Count == 0)
            throw new ArgumentException("El pedido debe contener al menos un producto.");

        if (orderItems.Any(item => item is null))
            throw new ArgumentException("El pedido contiene un producto inválido.");

        return new Order(customerId, orderItems);
    }

    public void ChangeStatus(OrderStatus newStatus)
    {
        var isValidTransition = Status switch
        {
            OrderStatus.Pending =>
                newStatus == OrderStatus.Confirmed ||
                newStatus == OrderStatus.Cancelled,

            OrderStatus.Confirmed =>
                newStatus == OrderStatus.Shipped ||
                newStatus == OrderStatus.Cancelled,

            _ => false
        };

        if (!isValidTransition)
            throw new InvalidOperationException(
                $"No se puede cambiar el estado de {Status} a {newStatus}.");

        Status = newStatus;
    }
}