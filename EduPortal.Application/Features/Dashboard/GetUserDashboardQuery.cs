using EduPortal.Application.Common;
using EduPortal.Application.Interfaces;
using EduPortal.Domain.Enums;
using MediatR;

namespace EduPortal.Application.Features.Dashboard;

public record GetUserDashboardQuery : IRequest<Result<UserDashboardDto>>;
public record UserDashboardDto(int TotalEnrollments, int CompletedExams, int PassedExams, int CertificatesEarned, List<EnrollmentSummary> RecentEnrollments);
public record EnrollmentSummary(Guid ResourceId, string ResourceTitle, DateTime EnrolledAt);

public class GetUserDashboardQueryHandler : IRequestHandler<GetUserDashboardQuery, Result<UserDashboardDto>>
{
    private readonly IEnrollmentRepository _enrollments;
    private readonly IExamRepository _exams;
    private readonly ICurrentUserService _currentUser;

    public GetUserDashboardQueryHandler(IEnrollmentRepository enrollments, IExamRepository exams, ICurrentUserService currentUser)
    {
        _enrollments = enrollments;
        _exams = exams;
        _currentUser = currentUser;
    }

    public async Task<Result<UserDashboardDto>> Handle(GetUserDashboardQuery request, CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId ?? Guid.Empty;
        var enrollments = await _enrollments.GetByUserIdAsync(userId, cancellationToken);
        var attempts = await _exams.GetAttemptsByUserIdAsync(userId, cancellationToken);
        var certs = await _exams.GetCertificatesByUserIdAsync(userId, cancellationToken);

        var recentEnrollments = enrollments
            .OrderByDescending(e => e.EnrolledAt)
            .Take(5)
            .Select(e => new EnrollmentSummary(e.ResourceId, e.Resource?.Title ?? "", e.EnrolledAt))
            .ToList();

        return Result<UserDashboardDto>.Success(new UserDashboardDto(
            enrollments.Count,
            attempts.Count(a => a.Status == AttemptStatus.Completed),
            attempts.Count(a => a.IsPassed == true),
            certs.Count,
            recentEnrollments));
    }
}
