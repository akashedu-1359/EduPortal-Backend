using EduPortal.Domain.Entities;

namespace EduPortal.Application.Interfaces;

public interface IEnrollmentRepository
{
    Task<bool> IsEnrolledAsync(Guid userId, Guid resourceId, CancellationToken ct = default);
    Task<List<Enrollment>> GetByUserIdAsync(Guid userId, CancellationToken ct = default);
    Task AddAsync(Enrollment enrollment, CancellationToken ct = default);
    Task SaveChangesAsync(CancellationToken ct = default);
}
