using EduPortal.Application.Interfaces;
using EduPortal.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace EduPortal.Infrastructure.Persistence.Repositories;

public class EnrollmentRepository : IEnrollmentRepository
{
    private readonly AppDbContext _db;

    public EnrollmentRepository(AppDbContext db) => _db = db;

    public Task<bool> IsEnrolledAsync(Guid userId, Guid resourceId, CancellationToken ct) =>
        _db.Enrollments.AnyAsync(e => e.UserId == userId && e.ResourceId == resourceId, ct);

    public Task<List<Enrollment>> GetByUserIdAsync(Guid userId, CancellationToken ct) =>
        _db.Enrollments.Include(e => e.Resource).Where(e => e.UserId == userId).ToListAsync(ct);

    public async Task AddAsync(Enrollment enrollment, CancellationToken ct) =>
        await _db.Enrollments.AddAsync(enrollment, ct);

    public Task SaveChangesAsync(CancellationToken ct) => _db.SaveChangesAsync(ct);
}
