using EduPortal.Application.Common;
using EduPortal.Application.Interfaces;
using EduPortal.Application.Features.Resources.Commands;
using EduPortal.Domain.Enums;
using MediatR;

namespace EduPortal.Application.Features.Resources.Queries;

public record GetPublicResourcesQuery(int Page = 1, int PageSize = 20, ResourceType? Type = null, Guid? CategoryId = null, bool? Featured = null, string? Search = null) : IRequest<Result<PagedResult<PublicResourceDto>>>;

public record PublicResourceDto(Guid Id, string Title, string Description, ResourceType ResourceType, decimal Price, bool IsFeatured, Guid CategoryId, string? CategoryName, string? ThumbnailUrl);

public class GetPublicResourcesQueryHandler : IRequestHandler<GetPublicResourcesQuery, Result<PagedResult<PublicResourceDto>>>
{
    private readonly IResourceRepository _resources;
    private readonly IStorageService _storage;
    private readonly ICacheService _cache;

    public GetPublicResourcesQueryHandler(IResourceRepository resources, IStorageService storage, ICacheService cache)
    { _resources = resources; _storage = storage; _cache = cache; }

    public async Task<Result<PagedResult<PublicResourceDto>>> Handle(GetPublicResourcesQuery request, CancellationToken cancellationToken)
    {
        var (items, total) = await _resources.GetPagedAsync(request.Page, request.PageSize, ResourceStatus.Published, request.CategoryId, cancellationToken);

        if (request.Type.HasValue) items = items.Where(r => r.ResourceType == request.Type.Value).ToList();
        if (request.Featured == true) items = items.Where(r => r.IsFeatured).ToList();
        if (!string.IsNullOrEmpty(request.Search))
        {
            var s = request.Search.ToLower();
            items = items.Where(r => r.Title.ToLower().Contains(s) || r.Description.ToLower().Contains(s)).ToList();
        }

        var dtos = new List<PublicResourceDto>();
        foreach (var r in items)
        {
            string? thumbnailUrl = null;
            if (!string.IsNullOrEmpty(r.ThumbnailKey))
                thumbnailUrl = await _storage.GetReadUrlAsync(r.ThumbnailKey, 3600, cancellationToken);
            dtos.Add(new PublicResourceDto(r.Id, r.Title, r.Description, r.ResourceType, r.Price, r.IsFeatured, r.CategoryId, r.Category?.Name, thumbnailUrl));
        }

        return Result<PagedResult<PublicResourceDto>>.Success(PagedResult<PublicResourceDto>.Create(dtos, request.Page, request.PageSize, total));
    }
}
