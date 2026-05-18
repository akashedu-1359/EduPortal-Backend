using EduPortal.Application.Common;
using EduPortal.Application.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

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
    private readonly ILogger<GetAdminDashboardQueryHandler> _logger;

    public GetAdminDashboardQueryHandler(IAnalyticsRepository analytics, ILogger<GetAdminDashboardQueryHandler> logger)
    {
        _analytics = analytics;
        _logger = logger;
    }

    public async Task<Result<AdminDashboardDto>> Handle(GetAdminDashboardQuery request, CancellationToken cancellationToken)
    {
        var users = await SafeCallAsync(() => _analytics.GetTotalUsersAsync(cancellationToken), nameof(_analytics.GetTotalUsersAsync));
        var enrollments = await SafeCallAsync(() => _analytics.GetTotalEnrollmentsAsync(cancellationToken), nameof(_analytics.GetTotalEnrollmentsAsync));
        var resources = await SafeCallAsync(() => _analytics.GetTotalResourcesAsync(cancellationToken), nameof(_analytics.GetTotalResourcesAsync));
        var published = await SafeCallAsync(() => _analytics.GetPublishedResourcesAsync(cancellationToken), nameof(_analytics.GetPublishedResourcesAsync));
        var exams = await SafeCallAsync(() => _analytics.GetTotalExamsAsync(cancellationToken), nameof(_analytics.GetTotalExamsAsync));
        var certs = await SafeCallAsync(() => _analytics.GetTotalCertificatesAsync(cancellationToken), nameof(_analytics.GetTotalCertificatesAsync));
        var revenue = await SafeCallAsync(() => _analytics.GetTotalRevenueAsync(cancellationToken), nameof(_analytics.GetTotalRevenueAsync));
        var revenueMonth = await SafeCallAsync(() => _analytics.GetRevenueThisMonthAsync(cancellationToken), nameof(_analytics.GetRevenueThisMonthAsync));

        return Result<AdminDashboardDto>.Success(new AdminDashboardDto(
            users, users, resources, published,
            enrollments, revenue, revenueMonth,
            certs, exams, []));
    }

    private async Task<int> SafeCallAsync(Func<Task<int>> fn, string name)
    {
        try { return await fn(); }
        catch (Exception ex) { _logger.LogError(ex, "Analytics query failed: {Method}", name); return 0; }
    }

    private async Task<decimal> SafeCallAsync(Func<Task<decimal>> fn, string name)
    {
        try { return await fn(); }
        catch (Exception ex) { _logger.LogError(ex, "Analytics query failed: {Method}", name); return 0m; }
    }
}
