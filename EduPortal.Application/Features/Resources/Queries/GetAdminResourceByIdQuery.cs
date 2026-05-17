using EduPortal.Application.Common;
using EduPortal.Application.Interfaces;
using EduPortal.Application.Features.Resources.Commands;
using MediatR;

namespace EduPortal.Application.Features.Resources.Queries;

public record GetAdminResourceByIdQuery(Guid Id) : IRequest<Result<ResourceDto>>;

public class GetAdminResourceByIdQueryHandler : IRequestHandler<GetAdminResourceByIdQuery, Result<ResourceDto>>
{
    private readonly IResourceRepository _resources;
    public GetAdminResourceByIdQueryHandler(IResourceRepository resources) => _resources = resources;

    public async Task<Result<ResourceDto>> Handle(GetAdminResourceByIdQuery request, CancellationToken cancellationToken)
    {
        var resource = await _resources.GetByIdAsync(request.Id, cancellationToken);
        if (resource == null) return Result<ResourceDto>.NotFound("Resource not found.");
        return Result<ResourceDto>.Success(CreateResourceCommandHandler.ToDto(resource));
    }
}
