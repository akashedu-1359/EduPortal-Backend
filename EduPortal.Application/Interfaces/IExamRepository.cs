using EduPortal.Domain.Entities;
using EduPortal.Domain.Enums;

namespace EduPortal.Application.Interfaces;

public interface IExamRepository
{
    Task<Exam?> GetByIdAsync(Guid id, bool includeQuestions = false, CancellationToken ct = default);
    Task<ExamScoringInfo?> GetExamScoringInfoAsync(Guid examId, CancellationToken ct = default);
    Task<ExamStatus?> GetExamStatusAsync(Guid examId, CancellationToken ct = default);
    Task<int> GetQuestionCountAsync(Guid examId, CancellationToken ct = default);
    Task AddQuestionAsync(Question question, CancellationToken ct = default);
    Task<(List<Exam> Items, int Total)> GetPagedAsync(int page, int pageSize, CancellationToken ct = default);
    Task<(List<Exam> Items, int Total)> GetPagedActiveAsync(int page, int pageSize, CancellationToken ct = default);
    Task<ExamAttempt?> GetAttemptAsync(Guid attemptId, CancellationToken ct = default);
    Task<int> GetAttemptCountAsync(Guid userId, Guid examId, CancellationToken ct = default);
    Task<(List<ExamAttempt> Items, int Total)> GetPagedAttemptsByUserIdAsync(Guid userId, int page, int pageSize, CancellationToken ct = default);
    Task<(List<ExamAttempt> Items, int Total)> GetPagedAttemptsAsync(int page, int pageSize, Guid? examId = null, CancellationToken ct = default);
    Task<ExamAttempt?> GetActiveAttemptAsync(Guid userId, Guid examId, CancellationToken ct = default);
    Task<Question?> GetQuestionByIdAsync(Guid id, CancellationToken ct = default);
    void RemoveQuestion(Question question);
    Task AddAsync(Exam exam, CancellationToken ct = default);
    Task AddAttemptAsync(ExamAttempt attempt, CancellationToken ct = default);
    Task AddAttemptAnswerAsync(AttemptAnswer answer, CancellationToken ct = default);
    Task SaveChangesAsync(CancellationToken ct = default);

    // Certificate methods
    Task AddCertificateAsync(Certificate cert, CancellationToken ct = default);
    Task<Certificate?> GetCertificateAsync(Guid id, CancellationToken ct = default);
    Task<List<Certificate>> GetCertificatesByUserIdAsync(Guid userId, CancellationToken ct = default);
    Task<(List<Certificate> Items, int Total)> GetPagedCertificatesAsync(int page, int pageSize, string? search = null, CancellationToken ct = default);
    Task<List<ExamAttempt>> GetAttemptsByUserIdAsync(Guid userId, CancellationToken ct = default);
}
