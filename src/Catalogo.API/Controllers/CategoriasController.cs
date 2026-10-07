using Catalogo.Application.Categorias.Commands;
using Catalogo.Application.Categorias.Queries;
using Catalogo.Application.Dtos;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Catalogo.API.Controllers;

[ApiController]
[Route("api/categorias")]
public sealed class CategoriasController(ISender sender) : ControllerBase
{
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Crear(CrearCategoriaCommand command, CancellationToken ct)
    {
        var id = await sender.Send(command, ct);
        return CreatedAtAction(nameof(ObtenerPorId), new { id }, new { id });
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<CategoriaDto>>> Listar(CancellationToken ct) =>
        Ok(await sender.Send(new ListarCategoriasQuery(), ct));

    [HttpGet("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CategoriaDto>> ObtenerPorId(Guid id, CancellationToken ct) =>
        Ok(await sender.Send(new ObtenerCategoriaPorIdQuery(id), ct));
}
