using Catalogo.Application.Abstractions;
using Catalogo.Domain.Entities;
using Catalogo.Domain.Exceptions;
using Catalogo.Domain.Repositories;
using MediatR;

namespace Catalogo.Application.Categorias.Commands;

public sealed record CrearCategoriaCommand(string Nombre) : IRequest<Guid>;

public sealed class CrearCategoriaHandler(ICategoriaRepository categorias, IUnitOfWork unitOfWork)
    : IRequestHandler<CrearCategoriaCommand, Guid>
{
    public async Task<Guid> Handle(CrearCategoriaCommand request, CancellationToken ct)
    {
        var categoria = Categoria.Crear(request.Nombre);

        if (await categorias.ExisteConNombreAsync(categoria.Nombre, ct))
            throw new DomainException($"Ya existe una categoría con el nombre '{categoria.Nombre}'.");

        categorias.Agregar(categoria);
        await unitOfWork.SaveChangesAsync(ct);

        return categoria.Id;
    }
}
