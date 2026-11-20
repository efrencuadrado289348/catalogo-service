
using Pedidos.Application.DTOs;
using Pedidos.Application.Interfaces;
using Pedidos.Domain.Entities;
using Pedidos.Domain.Enums;

namespace Pedidos.Application.Services;

public class OrderService(
    IOrderRepository orderRepository,
    IProductCatalogService productCatalogService)
{
    public async Task<OrderResponse> CreateOrderAsync(
        Guid customerId,
        CreateOrderRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (request.Items is null || request.Items.Count == 0)
            throw new ArgumentException(
                "El pedido debe contener al menos un producto.");

        var items = new List<OrderItem>();

        foreach (var item in request.Items)
        {
            var product = await productCatalogService.GetProductByIdAsync(
                item.ProductId,
                cancellationToken);

            if (product is null)
                throw new ArgumentException(
                    $"No se encontró el producto {item.ProductId}.");

            var orderItem = OrderItem.Create(
                product.Id,
                product.Name,
                item.Quantity,
                product.Price);

            items.Add(orderItem);
        }

        var order = Order.Create(customerId, items);

        await orderRepository.AddAsync(order, cancellationToken);
        await orderRepository.SaveChangesAsync(cancellationToken);

        return MapToResponse(order);
    }

    public async Task<IReadOnlyList<OrderResponse>> GetAllOrdersAsync(
        CancellationToken cancellationToken = default)
    {
        var orders = await orderRepository.GetAllAsync(cancellationToken);
        return orders.Select(MapToResponse).ToList();
    }

    public async Task<IReadOnlyList<OrderResponse>> GetMyOrdersAsync(
        Guid customerId,
        CancellationToken cancellationToken = default)
    {
        var orders = await orderRepository.GetByCustomerIdAsync(
            customerId, cancellationToken);

        return orders.Select(MapToResponse).ToList();
    }

    public async Task<OrderResponse?> GetOrderByIdAsync(
        Guid orderId,
        CancellationToken cancellationToken = default)
    {
        var order = await orderRepository.GetByIdAsync(
            orderId, cancellationToken);

        return order is null ? null : MapToResponse(order);
    }

    public async Task<OrderResponse?> ChangeOrderStatusAsync(
        Guid orderId,
        OrderStatus newStatus,
        CancellationToken cancellationToken = default)
    {
        var order = await orderRepository.GetByIdAsync(
            orderId, cancellationToken);

        if (order is null)
            return null;

        order.ChangeStatus(newStatus);

        await orderRepository.SaveChangesAsync(cancellationToken);

        return MapToResponse(order);
    }

    private static OrderResponse MapToResponse(Order order)
    {
        var items = order.Items.Select(item =>
            new OrderItemResponse(
                item.ProductId,
                item.ProductName,
                item.Quantity,
                item.UnitPrice,
                item.Subtotal
            )).ToList();

        return new OrderResponse(
            order.Id,
            order.CustomerId,
            order.CreatedAtUtc,
            order.Status,
            order.Total,
            items
        );
    }
}
