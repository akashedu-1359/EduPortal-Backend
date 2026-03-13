using EduPortal.Application.Common;
using EduPortal.Application.Interfaces;
using MediatR;

namespace EduPortal.Application.Features.Analytics.Queries;

public record GetAdminDashboardQuery : IRequest<Result<AdminDashboardDto>>;
public record AdminDashboardDto(int TotalUsers, int TotalEnrollments, int TotalResources, int TotalExamAttempts, decimal TotalRevenue, int TotalOrders);

public class GetAdminDashboardQueryHandler : IRequestHandler<GetAdminDashboardQuery, Result<AdminDashboardDto>>
{
    private readonly IAnalyticsRepository _analytics;

    public GetAdminDashboardQueryHandler(IAnalyticsRepository analytics) => _analytics = analytics;

    public async Task<Result<AdminDashboardDto>> Handle(GetAdminDashboardQuery request, CancellationToken cancellationToken)
    {
        var users = await _analytics.GetTotalUsersAsync(cancellationToken);
        var enrollments = await _analytics.GetTotalEnrollmentsAsync(cancellationToken);
        var resources = await _analytics.GetTotalResourcesAsync(cancellationToken);
        var attempts = await _analytics.GetTotalExamAttemptsAsync(cancellationToken);
        var revenue = await _analytics.GetTotalRevenueAsync(cancellationToken);
        var orders = await _analytics.GetTotalOrdersAsync(cancellationToken);
        return Result<AdminDashboardDto>.Success(new AdminDashboardDto(users, enrollments, resources, attempts, revenue, orders));
    }
}
