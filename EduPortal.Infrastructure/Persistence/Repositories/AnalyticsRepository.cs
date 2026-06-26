using EduPortal.Application.Interfaces;
using EduPortal.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace EduPortal.Infrastructure.Persistence.Repositories;

public class AnalyticsRepository : IAnalyticsRepository
{
    private readonly AppDbContext _db;

    public AnalyticsRepository(AppDbContext db) => _db = db;

    public Task<int> GetTotalUsersAsync(CancellationToken ct) => _db.Users.CountAsync(ct);
    public Task<int> GetTotalEnrollmentsAsync(CancellationToken ct) => _db.Enrollments.CountAsync(ct);
    public Task<int> GetTotalResourcesAsync(CancellationToken ct) => _db.Resources.CountAsync(r => !r.IsDeleted, ct);
    public Task<int> GetPublishedResourcesAsync(CancellationToken ct) => _db.Resources.CountAsync(r => !r.IsDeleted && r.Status == ResourceStatus.Published, ct);
    public Task<int> GetTotalExamsAsync(CancellationToken ct) => _db.Exams.CountAsync(ct);
    public Task<int> GetTotalCertificatesAsync(CancellationToken ct) => _db.Certificates.CountAsync(ct);

    public async Task<decimal> GetTotalRevenueAsync(CancellationToken ct) =>
        await _db.Orders.Where(o => o.Status == OrderStatus.Completed).SumAsync(o => (decimal?)o.Amount, ct) ?? 0m;

    public async Task<decimal> GetRevenueThisMonthAsync(CancellationToken ct)
    {
        var start = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1);
        return await _db.Orders.Where(o => o.Status == OrderStatus.Completed && o.CreatedAt >= start)
            .SumAsync(o => (decimal?)o.Amount, ct) ?? 0m;
    }

    public async Task<List<(DateOnly Date, decimal Amount)>> GetRevenueByDayAsync(int days, CancellationToken ct)
    {
        var from = DateTime.UtcNow.AddDays(-days).Date;
        var rows = await _db.Orders
            .Where(o => o.Status == OrderStatus.Completed && o.CreatedAt >= from)
            .Select(o => new { o.CreatedAt, o.Amount })
            .ToListAsync(ct);
        return rows
            .GroupBy(o => DateOnly.FromDateTime(o.CreatedAt))
            .Select(g => (g.Key, g.Sum(o => o.Amount)))
            .OrderBy(r => r.Key)
            .ToList();
    }

    public async Task<List<(DateOnly Date, int Count)>> GetEnrollmentsByDayAsync(int days, CancellationToken ct)
    {
        var from = DateTime.UtcNow.AddDays(-days).Date;
        var rows = await _db.Enrollments
            .Where(e => e.EnrolledAt >= from)
            .Select(e => new { e.EnrolledAt })
            .ToListAsync(ct);
        return rows
            .GroupBy(e => DateOnly.FromDateTime(e.EnrolledAt))
            .Select(g => (g.Key, g.Count()))
            .OrderBy(r => r.Key)
            .ToList();
    }
}
