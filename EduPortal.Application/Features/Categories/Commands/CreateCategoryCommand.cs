using EduPortal.Application.Common;
using EduPortal.Application.Interfaces;
using EduPortal.Domain.Entities;
using FluentValidation;
using MediatR;

namespace EduPortal.Application.Features.Categories.Commands;

public record CreateCategoryCommand(string Name, string? Description, bool IsVisible = true, int SortOrder = 0) : IRequest<Result<CategoryDto>>;

public record CategoryDto(Guid Id, string Name, string Slug, string? Description, bool IsVisible, int SortOrder);

public class CreateCategoryCommandValidator : AbstractValidator<CreateCategoryCommand>
{
    public CreateCategoryCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
    }
}

public class CreateCategoryCommandHandler : IRequestHandler<CreateCategoryCommand, Result<CategoryDto>>
{
    private readonly ICategoryRepository _categories;

    public CreateCategoryCommandHandler(ICategoryRepository categories) => _categories = categories;

    public async Task<Result<CategoryDto>> Handle(CreateCategoryCommand request, CancellationToken cancellationToken)
    {
        var slug = GenerateSlug(request.Name);
        var existing = await _categories.GetBySlugAsync(slug, cancellationToken);
        if (existing != null) return Result<CategoryDto>.Conflict($"A category with slug '{slug}' already exists.");

        var category = new Category(request.Name, slug, request.Description)
        {
            IsVisible = request.IsVisible,
            SortOrder = request.SortOrder
        };
        await _categories.AddAsync(category, cancellationToken);
        await _categories.SaveChangesAsync(cancellationToken);

        return Result<CategoryDto>.Created(ToDto(category));
    }

    public static string GenerateSlug(string name) =>
        System.Text.RegularExpressions.Regex.Replace(name.ToLowerInvariant().Trim(), @"[^a-z0-9]+", "-").Trim('-');

    public static CategoryDto ToDto(Category c) => new(c.Id, c.Name, c.Slug, c.Description, c.IsVisible, c.SortOrder);
}
