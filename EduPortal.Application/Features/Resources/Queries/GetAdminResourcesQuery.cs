using EduPortal.Application.Common;
using EduPortal.Application.Interfaces;
using EduPortal.Application.Features.Resources.Commands;
using EduPortal.Domain.Enums;
using MediatR;

namespace EduPortal.Application.Features.Resources.Queries;

public record GetAdminResourcesQuery(int Page = 1, int PageSize = 20, ResourceStatus? Status = null, Guid? CategoryId = null) : IRequest<Result<PagedResult<ResourceDto>>>;

public class GetAdminResourcesQueryHandler : IRequestHandler<GetAdminResourcesQuery, Result<PagedResult<ResourceDto>>>
{
    private readonly IResourceRepository _resources;
    public GetAdminResourcesQueryHandler(IResourceRepository resources) => _resources = resources;

    public async Task<Result<PagedResult<ResourceDto>>> Handle(GetAdminResourcesQuery request, CancellationToken cancellationToken)
    {
        var (items, total) = await _resources.GetPagedAsync(request.Page, request.PageSize, request.Status, request.CategoryId, cancellationToken);
        var dtos = items.Select(r => CreateResourceCommandHandler.ToDto(r)).ToList();
        return Result<PagedResult<ResourceDto>>.Success(PagedResult<ResourceDto>.Create(dtos, request.Page, request.PageSize, total));
    }
}
