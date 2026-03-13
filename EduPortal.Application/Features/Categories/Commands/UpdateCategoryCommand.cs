using EduPortal.Application.Common;
using EduPortal.Application.Interfaces;
using FluentValidation;
using MediatR;

namespace EduPortal.Application.Features.Categories.Commands;

public record UpdateCategoryCommand(Guid Id, string Name, string? Description, bool IsVisible, int SortOrder) : IRequest<Result<CategoryDto>>;

public class UpdateCategoryCommandValidator : AbstractValidator<UpdateCategoryCommand>
{
    public UpdateCategoryCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
    }
}

public class UpdateCategoryCommandHandler : IRequestHandler<UpdateCategoryCommand, Result<CategoryDto>>
{
    private readonly ICategoryRepository _categories;

    public UpdateCategoryCommandHandler(ICategoryRepository categories) => _categories = categories;

    public async Task<Result<CategoryDto>> Handle(UpdateCategoryCommand request, CancellationToken cancellationToken)
    {
        var category = await _categories.GetByIdAsync(request.Id, cancellationToken);
        if (category == null) return Result<CategoryDto>.NotFound("Category not found.");

        var newSlug = CreateCategoryCommandHandler.GenerateSlug(request.Name);
        if (newSlug != category.Slug)
        {
            var conflict = await _categories.GetBySlugAsync(newSlug, cancellationToken);
            if (conflict != null && conflict.Id != request.Id)
                return Result<CategoryDto>.Conflict($"Slug '{newSlug}' is already in use.");
            category.Slug = newSlug;
        }

        category.Name = request.Name;
        category.Description = request.Description;
        category.IsVisible = request.IsVisible;
        category.SortOrder = request.SortOrder;
        await _categories.SaveChangesAsync(cancellationToken);

        return Result<CategoryDto>.Success(CreateCategoryCommandHandler.ToDto(category));
    }
}
