namespace EduPortal.Application.Interfaces;

public interface IAnalyticsRepository
{
    Task<int> GetTotalUsersAsync(CancellationToken ct = default);
    Task<int> GetTotalEnrollmentsAsync(CancellationToken ct = default);
    Task<int> GetTotalResourcesAsync(CancellationToken ct = default);
    Task<int> GetTotalExamAttemptsAsync(CancellationToken ct = default);
    Task<decimal> GetTotalRevenueAsync(CancellationToken ct = default);
    Task<int> GetTotalOrdersAsync(CancellationToken ct = default);
}
