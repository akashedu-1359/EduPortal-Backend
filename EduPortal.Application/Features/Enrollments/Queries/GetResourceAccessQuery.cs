using EduPortal.Application.Common;
using EduPortal.Application.Interfaces;
using EduPortal.Domain.Entities;
using EduPortal.Domain.Enums;
using MediatR;

namespace EduPortal.Application.Features.Enrollments.Queries;

public record GetResourceAccessQuery(Guid ResourceId) : IRequest<Result<ResourceAccessDto>>;
public record ResourceAccessDto(string? AccessUrl, ResourceType ResourceType, DateTime? ExpiresAt);

public class GetResourceAccessQueryHandler : IRequestHandler<GetResourceAccessQuery, Result<ResourceAccessDto>>
{
    private readonly IResourceRepository _resources;
    private readonly IEnrollmentRepository _enrollments;
    private readonly ICurrentUserService _currentUser;
    private readonly IStorageService _storage;

    public GetResourceAccessQueryHandler(IResourceRepository resources, IEnrollmentRepository enrollments, ICurrentUserService currentUser, IStorageService storage)
    { _resources = resources; _enrollments = enrollments; _currentUser = currentUser; _storage = storage; }

    public async Task<Result<ResourceAccessDto>> Handle(GetResourceAccessQuery request, CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId ?? Guid.Empty;
        var resource = await _resources.GetByIdAsync(request.ResourceId, cancellationToken);
        if (resource == null) return Result<ResourceAccessDto>.NotFound("Resource not found.");

        // Check entitlement
        bool entitled;
        if (resource.Price == 0)
        {
            entitled = true;
            var alreadyEnrolled = await _enrollments.IsEnrolledAsync(userId, request.ResourceId, cancellationToken);
            if (!alreadyEnrolled)
            {
                await _enrollments.AddAsync(Enrollment.Create(userId, request.ResourceId), cancellationToken);
                await _enrollments.SaveChangesAsync(cancellationToken);
            }
        }
        else
        {
            entitled = await _enrollments.IsEnrolledAsync(userId, request.ResourceId, cancellationToken);
        }

        if (!entitled) return Result<ResourceAccessDto>.Failure("Purchase required to access this resource.", 403);

        if (resource.ResourceType == ResourceType.Blog)
            return Result<ResourceAccessDto>.Failure("Blog content is available on the resource detail page.", 400);

        if (!string.IsNullOrEmpty(resource.ExternalUrl))
            return Result<ResourceAccessDto>.Success(new ResourceAccessDto(resource.ExternalUrl, resource.ResourceType, null));

        if (string.IsNullOrEmpty(resource.FileKey))
            return Result<ResourceAccessDto>.Failure("Resource file not available.", 400);

        var expiry = resource.ResourceType == ResourceType.Video ? 900 : 300; // 15min video, 5min pdf
        var url = await _storage.GetReadUrlAsync(resource.FileKey, expiry, cancellationToken);
        return Result<ResourceAccessDto>.Success(new ResourceAccessDto(url, resource.ResourceType, DateTime.UtcNow.AddSeconds(expiry)));
    }
}
