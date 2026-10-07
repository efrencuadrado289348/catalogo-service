using Catalog.Application.Abstractions;
using Catalog.Domain.Entities;
using Catalog.Domain.Exceptions;
using Catalog.Domain.Repositories;
using MediatR;

namespace Catalog.Application.Categories.Commands;

public sealed record CreateCategoryCommand(string Name, string? Description = null) : IRequest<Guid>;

public sealed class CreateCategoryHandler(ICategoryRepository categories, IUnitOfWork unitOfWork)
    : IRequestHandler<CreateCategoryCommand, Guid>
{
    public async Task<Guid> Handle(CreateCategoryCommand request, CancellationToken ct)
    {
        var category = Category.Create(request.Name, request.Description);

        if (await categories.ExistsByNameAsync(category.Name, ct))
            throw new DomainException($"A category with the name '{category.Name}' already exists.");

        await categories.AddAsync(category, ct);
        await unitOfWork.SaveChangesAsync(ct);

        return category.Id;
    }
}
