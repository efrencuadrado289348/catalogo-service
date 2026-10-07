using Catalog.API.Contracts;
using Catalog.Application.Dtos;
using Catalog.Application.Products.Commands;
using Catalog.Application.Products.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Catalog.API.Controllers;

[ApiController]
[Route("api/products")]
public sealed class ProductsController(ISender sender) : ControllerBase
{
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create(CreateProductCommand command, CancellationToken ct)
    {
        var id = await sender.Send(command, ct);
        return CreatedAtAction(nameof(GetById), new { id }, new { id });
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<ProductDto>>> List([FromQuery] Guid? categoryId, CancellationToken ct) =>
        Ok(await sender.Send(new ListProductsQuery(categoryId), ct));

    [HttpGet("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ProductDto>> GetById(Guid id, CancellationToken ct) =>
        Ok(await sender.Send(new GetProductByIdQuery(id), ct));

    [HttpPatch("{id:guid}/price")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdatePrice(Guid id, UpdatePriceRequest request, CancellationToken ct)
    {
        await sender.Send(new UpdatePriceCommand(id, request.Amount, request.Currency), ct);
        return NoContent();
    }

    [HttpPatch("{id:guid}/stock")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AdjustStock(Guid id, AdjustStockRequest request, CancellationToken ct)
    {
        await sender.Send(new AdjustStockCommand(id, request.Quantity), ct);
        return NoContent();
    }

    [HttpPatch("{id:guid}/deactivate")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Deactivate(Guid id, CancellationToken ct)
    {
        await sender.Send(new DeactivateProductCommand(id), ct);
        return NoContent();
    }
}
