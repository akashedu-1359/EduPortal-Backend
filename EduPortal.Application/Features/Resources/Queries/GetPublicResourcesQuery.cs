using EduPortal.Application.Common;
using EduPortal.Application.Interfaces;
using EduPortal.Application.Features.Resources.Commands;
using EduPortal.Domain.Enums;
using MediatR;

namespace EduPortal.Application.Features.Resources.Queries;

public record GetPublicResourcesQuery(int PageNumber = 1, int PageSize = 20, ResourceType? Type = null, Guid? CategoryId = null, bool? Featured = null, string? Search = null) : IRequest<Result<PagedResult<ResourceDto>>>;

public class GetPublicResourcesQueryHandler : IRequestHandler<GetPublicResourcesQuery, Result<PagedResult<ResourceDto>>>
{
    private readonly IResourceRepository _resources;
    private readonly IStorageService _storage;
    private readonly ICacheService _cache;

    public GetPublicResourcesQueryHandler(IResourceRepository resources, IStorageService storage, ICacheService cache)
    { _resources = resources; _storage = storage; _cache = cache; }

    public async Task<Result<PagedResult<ResourceDto>>> Handle(GetPublicResourcesQuery request, CancellationToken cancellationToken)
    {
        var (items, total) = await _resources.GetPagedAsync(request.PageNumber, request.PageSize, ResourceStatus.Published, request.CategoryId, cancellationToken);

        if (request.Type.HasValue) items = items.Where(r => r.ResourceType == request.Type.Value).ToList();
        if (request.Featured == true) items = items.Where(r => r.IsFeatured).ToList();
        if (!string.IsNullOrEmpty(request.Search))
        {
            var s = request.Search.ToLower();
            items = items.Where(r => r.Title.ToLower().Contains(s) || r.Description.ToLower().Contains(s)).ToList();
        }

        var dtos = new List<ResourceDto>();
        foreach (var r in items)
        {
            string? thumbnailUrl = null;
            if (!string.IsNullOrEmpty(r.ThumbnailKey))
                thumbnailUrl = await _storage.GetReadUrlAsync(r.ThumbnailKey, 3600, cancellationToken);
            dtos.Add(CreateResourceCommandHandler.ToDto(r, r.Category, thumbnailUrl));
        }

        return Result<PagedResult<ResourceDto>>.Success(PagedResult<ResourceDto>.Create(dtos, request.PageNumber, request.PageSize, total));
    }
}
