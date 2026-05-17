namespace EduPortal.Application.Interfaces;

public interface IAnalyticsRepository
{
    Task<int> GetTotalUsersAsync(CancellationToken ct = default);
    Task<int> GetTotalEnrollmentsAsync(CancellationToken ct = default);
    Task<int> GetTotalResourcesAsync(CancellationToken ct = default);
    Task<int> GetPublishedResourcesAsync(CancellationToken ct = default);
    Task<int> GetTotalExamsAsync(CancellationToken ct = default);
    Task<int> GetTotalCertificatesAsync(CancellationToken ct = default);
    Task<decimal> GetTotalRevenueAsync(CancellationToken ct = default);
    Task<decimal> GetRevenueThisMonthAsync(CancellationToken ct = default);
    Task<List<(DateOnly Date, decimal Amount)>> GetRevenueByDayAsync(int days, CancellationToken ct = default);
    Task<List<(DateOnly Date, int Count)>> GetEnrollmentsByDayAsync(int days, CancellationToken ct = default);
}
