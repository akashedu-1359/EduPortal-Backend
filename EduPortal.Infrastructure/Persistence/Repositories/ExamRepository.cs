using EduPortal.Application.Interfaces;
using EduPortal.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace EduPortal.Infrastructure.Persistence.Repositories;

public class ExamRepository : IExamRepository
{
    private readonly AppDbContext _db;

    public ExamRepository(AppDbContext db) => _db = db;

    public Task<Exam?> GetByIdAsync(Guid id, bool includeQuestions = false, CancellationToken ct = default)
    {
        var query = _db.Exams.Where(e => e.Id == id && !e.IsDeleted);
        if (includeQuestions) query = query.Include(e => e.Questions);
        return query.FirstOrDefaultAsync(ct);
    }

    public async Task<(List<Exam> Items, int Total)> GetPagedAsync(int page, int pageSize, CancellationToken ct = default)
    {
        var query = _db.Exams.Where(e => !e.IsDeleted).Include(e => e.Questions);
        var total = await query.CountAsync(ct);
        var items = await query.OrderByDescending(e => e.CreatedAt).Skip((page - 1) * pageSize).Take(pageSize).ToListAsync(ct);
        return (items, total);
    }

    public Task<ExamAttempt?> GetAttemptAsync(Guid attemptId, CancellationToken ct) =>
        _db.ExamAttempts.Include(a => a.Answers).FirstOrDefaultAsync(a => a.Id == attemptId, ct);

    public Task<int> GetAttemptCountAsync(Guid userId, Guid examId, CancellationToken ct) =>
        _db.ExamAttempts.CountAsync(a => a.UserId == userId && a.ExamId == examId, ct);

    public async Task AddAsync(Exam exam, CancellationToken ct) =>
        await _db.Exams.AddAsync(exam, ct);

    public async Task AddAttemptAsync(ExamAttempt attempt, CancellationToken ct) =>
        await _db.ExamAttempts.AddAsync(attempt, ct);

    public async Task AddCertificateAsync(Certificate cert, CancellationToken ct) =>
        await _db.Certificates.AddAsync(cert, ct);

    public Task<Certificate?> GetCertificateAsync(Guid id, CancellationToken ct) =>
        _db.Certificates.FirstOrDefaultAsync(c => c.Id == id, ct);

    public Task<List<Certificate>> GetCertificatesByUserIdAsync(Guid userId, CancellationToken ct) =>
        _db.Certificates.Where(c => c.UserId == userId).OrderByDescending(c => c.IssuedAt).ToListAsync(ct);

    public Task<List<ExamAttempt>> GetAttemptsByUserIdAsync(Guid userId, CancellationToken ct) =>
        _db.ExamAttempts.Where(a => a.UserId == userId).ToListAsync(ct);

    public Task SaveChangesAsync(CancellationToken ct) => _db.SaveChangesAsync(ct);
}
