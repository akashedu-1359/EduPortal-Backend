using EduPortal.Application.Common;
using EduPortal.Application.Features.Resources.Commands;
using EduPortal.Application.Interfaces;
using MediatR;

namespace EduPortal.Application.Features.Enrollments.Queries;

public record GetUserEnrollmentsQuery : IRequest<Result<List<UserEnrollmentDto>>>;

public record UserEnrollmentDto(
    Guid Id,
    Guid UserId,
    Guid ResourceId,
    ResourceDto Resource,
    DateTime EnrolledAt,
    DateTime? LastAccessedAt,
    int Progress);

public class GetUserEnrollmentsQueryHandler : IRequestHandler<GetUserEnrollmentsQuery, Result<List<UserEnrollmentDto>>>
{
    private readonly IEnrollmentRepository _enrollments;
    private readonly IStorageService _storage;
    private readonly ICurrentUserService _currentUser;

    public GetUserEnrollmentsQueryHandler(
        IEnrollmentRepository enrollments,
        IStorageService storage,
        ICurrentUserService currentUser)
    {
        _enrollments = enrollments;
        _storage = storage;
        _currentUser = currentUser;
    }

    public async Task<Result<List<UserEnrollmentDto>>> Handle(GetUserEnrollmentsQuery request, CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId ?? Guid.Empty;
        var enrollments = await _enrollments.GetByUserIdAsync(userId, cancellationToken);

        var dtos = new List<UserEnrollmentDto>();
        foreach (var enrollment in enrollments.OrderByDescending(e => e.EnrolledAt))
        {
            string? thumbnailUrl = null;
            if (!string.IsNullOrEmpty(enrollment.Resource?.ThumbnailKey))
            {
                thumbnailUrl = await _storage.GetReadUrlAsync(
                    enrollment.Resource.ThumbnailKey,
                    3600,
                    cancellationToken);
            }

            var resourceDto = enrollment.Resource != null
                ? CreateResourceCommandHandler.ToDto(enrollment.Resource, enrollment.Resource.Category, thumbnailUrl)
                : null!;

            dtos.Add(new UserEnrollmentDto(
                enrollment.Id,
                enrollment.UserId,
                enrollment.ResourceId,
                resourceDto,
                enrollment.EnrolledAt,
                null,
                0));
        }

        return Result<List<UserEnrollmentDto>>.Success(dtos);
    }
}
