
namespace Pedidos.Application.DTOs;

public record CreateOrderItemRequest(
    Guid ProductId,
    int Quantity
);
