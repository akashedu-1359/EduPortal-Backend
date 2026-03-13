using EduPortal.Domain.Entities;

namespace EduPortal.Application.Interfaces;

public interface IExamRepository
{
    Task<Exam?> GetByIdAsync(Guid id, bool includeQuestions = false, CancellationToken ct = default);
    Task<(List<Exam> Items, int Total)> GetPagedAsync(int page, int pageSize, CancellationToken ct = default);
    Task<ExamAttempt?> GetAttemptAsync(Guid attemptId, CancellationToken ct = default);
    Task<int> GetAttemptCountAsync(Guid userId, Guid examId, CancellationToken ct = default);
    Task AddAsync(Exam exam, CancellationToken ct = default);
    Task AddAttemptAsync(ExamAttempt attempt, CancellationToken ct = default);
    Task SaveChangesAsync(CancellationToken ct = default);

    // Certificate methods
    Task AddCertificateAsync(Certificate cert, CancellationToken ct = default);
    Task<Certificate?> GetCertificateAsync(Guid id, CancellationToken ct = default);
    Task<List<Certificate>> GetCertificatesByUserIdAsync(Guid userId, CancellationToken ct = default);
    Task<List<ExamAttempt>> GetAttemptsByUserIdAsync(Guid userId, CancellationToken ct = default);
}
