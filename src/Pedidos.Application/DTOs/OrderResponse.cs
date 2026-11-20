using Pedidos.Domain.Enums;

namespace Pedidos.Application.DTOs;

public record OrderResponse(
    Guid Id,
    Guid CustomerId,
    DateTime CreatedAtUtc,
    OrderStatus Status,
    decimal Total,
    IReadOnlyList<OrderItemResponse> Items
);

public record OrderItemResponse(
    Guid ProductId,
    string ProductName,
    int Quantity,
    decimal UnitPrice,
    decimal Subtotal
);