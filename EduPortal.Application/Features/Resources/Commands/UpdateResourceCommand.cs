using EduPortal.Application.Common;
using EduPortal.Application.Interfaces;
using EduPortal.Domain.Enums;
using MediatR;

namespace EduPortal.Application.Features.Resources.Commands;

public record UpdateResourceCommand(Guid Id, string Title, string Description,
    string? FileKey, string? ExternalUrl, string? BlogContent, string? ThumbnailKey,
    decimal Price, bool IsFeatured, Guid CategoryId) : IRequest<Result<ResourceDto>>;

public class UpdateResourceCommandHandler : IRequestHandler<UpdateResourceCommand, Result<ResourceDto>>
{
    private readonly IResourceRepository _resources;
    private readonly ICategoryRepository _categories;

    public UpdateResourceCommandHandler(IResourceRepository resources, ICategoryRepository categories)
    {
        _resources = resources;
        _categories = categories;
    }

    public async Task<Result<ResourceDto>> Handle(UpdateResourceCommand request, CancellationToken cancellationToken)
    {
        var resource = await _resources.GetByIdAsync(request.Id, cancellationToken);
        if (resource == null) return Result<ResourceDto>.NotFound("Resource not found.");

        var category = await _categories.GetByIdAsync(request.CategoryId, cancellationToken);
        if (category == null) return Result<ResourceDto>.NotFound("Category not found.");

        resource.Title = request.Title;
        resource.Description = request.Description;
        resource.FileKey = request.FileKey;
        resource.ExternalUrl = request.ExternalUrl;
        resource.BlogContent = request.BlogContent;
        resource.ThumbnailKey = request.ThumbnailKey;
        resource.Price = request.Price;
        resource.IsFeatured = request.IsFeatured;
        resource.CategoryId = request.CategoryId;

        await _resources.SaveChangesAsync(cancellationToken);
        return Result<ResourceDto>.Success(CreateResourceCommandHandler.ToDto(resource));
    }
}
