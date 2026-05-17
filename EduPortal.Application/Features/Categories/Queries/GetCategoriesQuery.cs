using EduPortal.Application.Common;
using EduPortal.Application.Interfaces;
using EduPortal.Application.Features.Categories.Commands;
using MediatR;

namespace EduPortal.Application.Features.Categories.Queries;

public record GetCategoriesQuery : IRequest<Result<List<CategoryDto>>>;

public class GetCategoriesQueryHandler : IRequestHandler<GetCategoriesQuery, Result<List<CategoryDto>>>
{
    private readonly ICategoryRepository _categories;

    public GetCategoriesQueryHandler(ICategoryRepository categories) => _categories = categories;

    public async Task<Result<List<CategoryDto>>> Handle(GetCategoriesQuery request, CancellationToken cancellationToken)
    {
        var categories = await _categories.GetAllAsync(cancellationToken);
        return Result<List<CategoryDto>>.Success(categories.Select(CreateCategoryCommandHandler.ToDto).ToList());
    }
}

public record GetPublicCategoriesQuery : IRequest<Result<List<CategoryDto>>>;

public class GetPublicCategoriesQueryHandler : IRequestHandler<GetPublicCategoriesQuery, Result<List<CategoryDto>>>
{
    private readonly ICategoryRepository _categories;

    public GetPublicCategoriesQueryHandler(ICategoryRepository categories) => _categories = categories;

    public async Task<Result<List<CategoryDto>>> Handle(GetPublicCategoriesQuery request, CancellationToken cancellationToken)
    {
        var categories = await _categories.GetVisibleAsync(cancellationToken);
        return Result<List<CategoryDto>>.Success(categories.Select(CreateCategoryCommandHandler.ToDto).ToList());
    }
}
