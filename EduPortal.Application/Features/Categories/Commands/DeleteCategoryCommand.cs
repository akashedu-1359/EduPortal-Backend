using EduPortal.Application.Common;
using EduPortal.Application.Interfaces;
using MediatR;

namespace EduPortal.Application.Features.Categories.Commands;

public record DeleteCategoryCommand(Guid Id) : IRequest<Result>;

public class DeleteCategoryCommandHandler : IRequestHandler<DeleteCategoryCommand, Result>
{
    private readonly ICategoryRepository _categories;
    private readonly IResourceRepository _resources;

    public DeleteCategoryCommandHandler(ICategoryRepository categories, IResourceRepository resources)
    {
        _categories = categories;
        _resources = resources;
    }

    public async Task<Result> Handle(DeleteCategoryCommand request, CancellationToken cancellationToken)
    {
        var category = await _categories.GetByIdAsync(request.Id, cancellationToken);
        if (category == null) return Result.NotFound("Category not found.");

        var (resources, total) = await _resources.GetPagedAsync(1, 1, categoryId: request.Id, ct: cancellationToken);
        if (total > 0) return Result.Failure("Cannot delete category with assigned resources.", 409);

        _categories.Remove(category);
        await _categories.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
