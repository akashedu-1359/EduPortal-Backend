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
    public Task<int> GetTotalResourcesAsync(CancellationToken ct) => _db.Resources.CountAsync(ct);
    public Task<int> GetTotalExamAttemptsAsync(CancellationToken ct) => _db.ExamAttempts.CountAsync(ct);
    public Task<decimal> GetTotalRevenueAsync(CancellationToken ct) =>
        _db.Orders.Where(o => o.Status == OrderStatus.Completed).SumAsync(o => o.Amount, ct);
    public Task<int> GetTotalOrdersAsync(CancellationToken ct) => _db.Orders.CountAsync(ct);
}
