using Catalogo.API.Contracts;
using Catalogo.Application.Dtos;
using Catalogo.Application.Productos.Commands;
using Catalogo.Application.Productos.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Catalogo.API.Controllers;

[ApiController]
[Route("api/productos")]
public sealed class ProductosController(ISender sender) : ControllerBase
{
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Crear(CrearProductoCommand command, CancellationToken ct)
    {
        var id = await sender.Send(command, ct);
        return CreatedAtAction(nameof(ObtenerPorId), new { id }, new { id });
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<ProductoDto>>> Listar([FromQuery] Guid? categoriaId, CancellationToken ct) =>
        Ok(await sender.Send(new ListarProductosQuery(categoriaId), ct));

    [HttpGet("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ProductoDto>> ObtenerPorId(Guid id, CancellationToken ct) =>
        Ok(await sender.Send(new ObtenerProductoPorIdQuery(id), ct));

    [HttpPatch("{id:guid}/precio")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ActualizarPrecio(Guid id, ActualizarPrecioRequest request, CancellationToken ct)
    {
        await sender.Send(new ActualizarPrecioCommand(id, request.Valor, request.Moneda), ct);
        return NoContent();
    }

    [HttpPatch("{id:guid}/stock")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AjustarStock(Guid id, AjustarStockRequest request, CancellationToken ct)
    {
        await sender.Send(new AjustarStockCommand(id, request.Cantidad), ct);
        return NoContent();
    }

    [HttpPatch("{id:guid}/desactivar")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Desactivar(Guid id, CancellationToken ct)
    {
        await sender.Send(new DesactivarProductoCommand(id), ct);
        return NoContent();
    }
}
