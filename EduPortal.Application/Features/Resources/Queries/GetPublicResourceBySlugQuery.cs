using EduPortal.Application.Common;
using EduPortal.Application.Interfaces;
using EduPortal.Application.Features.Resources.Commands;
using MediatR;

namespace EduPortal.Application.Features.Resources.Queries;

public record ResourceDetailDto(
    Guid Id, string Title, string Slug, string Description, string? ThumbnailUrl,
    Domain.Enums.ResourceType Type, Domain.Enums.ResourceStatus Status,
    string PricingType, decimal Price, string? Currency,
    Guid CategoryId, ResourceCategoryDto? Category,
    string[] Tags, int? DurationMinutes,
    Guid AuthorId, string AuthorName,
    int ViewCount, int EnrollmentCount,
    bool IsPublished, DateTime? PublishedAt,
    DateTime CreatedAt, DateTime UpdatedAt,
    string? ContentUrl, bool IsEnrolled, bool HasPurchased,
    List<ResourceDto> RelatedResources);

public record GetPublicResourceBySlugQuery(string Slug, Guid? UserId) : IRequest<Result<ResourceDetailDto>>;

public class GetPublicResourceBySlugQueryHandler : IRequestHandler<GetPublicResourceBySlugQuery, Result<ResourceDetailDto>>
{
    private readonly IResourceRepository _resources;
    private readonly IStorageService _storage;

    public GetPublicResourceBySlugQueryHandler(IResourceRepository resources, IStorageService storage)
    { _resources = resources; _storage = storage; }

    public async Task<Result<ResourceDetailDto>> Handle(GetPublicResourceBySlugQuery request, CancellationToken cancellationToken)
    {
        var resource = await _resources.GetBySlugAsync(request.Slug, cancellationToken);
        if (resource == null) return Result<ResourceDetailDto>.NotFound("Resource not found.");

        string? thumbnailUrl = null;
        if (!string.IsNullOrEmpty(resource.ThumbnailKey))
            thumbnailUrl = await _storage.GetReadUrlAsync(resource.ThumbnailKey, 3600, cancellationToken);

        var isFree = resource.Price == 0;
        var isEnrolled = resource.Enrollments?.Any(e => e.UserId == request.UserId) ?? false;

        string? contentUrl = null;
        if ((isFree || isEnrolled) && !string.IsNullOrEmpty(resource.FileKey))
            contentUrl = await _storage.GetReadUrlAsync(resource.FileKey, 3600, cancellationToken);

        var (related, _) = await _resources.GetPagedAsync(1, 5, Domain.Enums.ResourceStatus.Published, resource.CategoryId, cancellationToken);
        var relatedDtos = related
            .Where(r => r.Id != resource.Id)
            .Take(4)
            .Select(r => CreateResourceCommandHandler.ToDto(r, r.Category))
            .ToList();

        var slug = CreateResourceCommandHandler.GenerateSlug(resource.Title);
        var cat = resource.Category != null
            ? new ResourceCategoryDto(resource.Category.Id, resource.Category.Name, resource.Category.Slug, resource.Category.Description, resource.Category.IsVisible)
            : null;

        var dto = new ResourceDetailDto(
            resource.Id, resource.Title, slug, resource.Description, thumbnailUrl,
            resource.ResourceType, resource.Status,
            isFree ? "Free" : "Paid", resource.Price, "INR",
            resource.CategoryId, cat,
            Array.Empty<string>(), null,
            resource.CreatedByAdminId, resource.CreatedByAdmin?.FullName ?? string.Empty,
            0, resource.Enrollments?.Count ?? 0,
            resource.Status == Domain.Enums.ResourceStatus.Published, null,
            resource.CreatedAt, resource.UpdatedAt,
            contentUrl, isEnrolled, isEnrolled,
            relatedDtos);

        return Result<ResourceDetailDto>.Success(dto);
    }
}
