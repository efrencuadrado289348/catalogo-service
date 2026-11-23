
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Pedidos.Application.DTOs;
using Pedidos.Application.Services;
using Pedidos.Domain.Enums;

namespace Pedidos.API.Controllers;

[ApiController]
[Route("api/orders")]
[Authorize]
public class OrdersController(OrderService orderService) : ControllerBase
{
    [HttpPost]
    [Authorize(Roles = "Cliente")]
    public async Task<IActionResult> Create(
        CreateOrderRequest request,
        CancellationToken cancellationToken)
    {
        if (!TryGetCustomerId(out var customerId))
            return Unauthorized();

        try
        {
            var order = await orderService.CreateOrderAsync(
                customerId, request, cancellationToken);

            return CreatedAtAction(
                nameof(GetById),
                new { id = order.Id },
                order);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpGet]
    [Authorize(Roles = "Admin,Vendedor,Cliente")]
    public async Task<IActionResult> GetAll(
        CancellationToken cancellationToken)
    {
        if (User.IsInRole("Cliente"))
        {
            if (!TryGetCustomerId(out var customerId))
                return Unauthorized();

            var ownOrders = await orderService.GetMyOrdersAsync(
                customerId, cancellationToken);

            return Ok(ownOrders);
        }

        var orders = await orderService.GetAllOrdersAsync(cancellationToken);
        return Ok(orders);
    }

    [HttpGet("{id:guid}")]
    [Authorize(Roles = "Admin,Vendedor,Cliente")]
    public async Task<IActionResult> GetById(
        Guid id,
        CancellationToken cancellationToken)
    {
        var order = await orderService.GetOrderByIdAsync(id, cancellationToken);

        if (order is null)
            return NotFound();

        if (User.IsInRole("Cliente"))
        {
            if (!TryGetCustomerId(out var customerId))
                return Unauthorized();

            if (order.CustomerId != customerId)
                return Forbid();
        }

        return Ok(order);
    }

    [HttpPatch("{id:guid}/status")]
    [Authorize(Roles = "Vendedor")]
    public async Task<IActionResult> ChangeStatus(
        Guid id,
        ChangeOrderStatusRequest request,
        CancellationToken cancellationToken)
    {
        if (!Enum.TryParse<OrderStatus>(
                request.Status, true, out var newStatus) ||
            !Enum.IsDefined(newStatus))
        {
            return BadRequest(new { message = "El estado indicado no es válido." });
        }

        try
        {
            var order = await orderService.ChangeOrderStatusAsync(
                id, newStatus, cancellationToken);

            return order is null ? NotFound() : Ok(order);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    private bool TryGetCustomerId(out Guid customerId)
    {
        var customerIdClaim = User.FindFirst("sub")?.Value;
        return Guid.TryParse(customerIdClaim, out customerId);
    }
}
