using EduPortal.Application.Common;
using EduPortal.Application.Interfaces;
using MediatR;

namespace EduPortal.Application.Features.Analytics.Queries;

public record GetAdminDashboardQuery : IRequest<Result<AdminDashboardDto>>;

public record AdminDashboardDto(
    int TotalUsers, int ActiveUsers,
    int TotalResources, int PublishedResources,
    int TotalEnrollments,
    decimal TotalRevenue, decimal RevenueThisMonth,
    int TotalCertificates, int TotalExams,
    List<ActivityItem> RecentActivity);

public record ActivityItem(string Id, string Type, string Description, string UserId, string UserName, DateTime CreatedAt);

public class GetAdminDashboardQueryHandler : IRequestHandler<GetAdminDashboardQuery, Result<AdminDashboardDto>>
{
    private readonly IAnalyticsRepository _analytics;

    public GetAdminDashboardQueryHandler(IAnalyticsRepository analytics) => _analytics = analytics;

    public async Task<Result<AdminDashboardDto>> Handle(GetAdminDashboardQuery request, CancellationToken cancellationToken)
    {
        var users = await _analytics.GetTotalUsersAsync(cancellationToken);
        var enrollments = await _analytics.GetTotalEnrollmentsAsync(cancellationToken);
        var resources = await _analytics.GetTotalResourcesAsync(cancellationToken);
        var published = await _analytics.GetPublishedResourcesAsync(cancellationToken);
        var exams = await _analytics.GetTotalExamsAsync(cancellationToken);
        var certs = await _analytics.GetTotalCertificatesAsync(cancellationToken);
        var revenue = await _analytics.GetTotalRevenueAsync(cancellationToken);
        var revenueMonth = await _analytics.GetRevenueThisMonthAsync(cancellationToken);

        return Result<AdminDashboardDto>.Success(new AdminDashboardDto(
            users, users, resources, published,
            enrollments, revenue, revenueMonth,
            certs, exams, []));
    }
}
