namespace Pedidos.Application.DTOs;

public record CreateOrderRequest(
    List<CreateOrderItemRequest> Items
);