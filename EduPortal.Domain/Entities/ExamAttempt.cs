using EduPortal.Domain.Common;
using EduPortal.Domain.Enums;
using EduPortal.Domain.Events;

namespace EduPortal.Domain.Entities;

public class ExamAttempt : BaseEntity
{
    public Guid UserId { get; private set; }
    public Guid ExamId { get; private set; }
    public DateTime StartedAt { get; private set; }
    public DateTime? CompletedAt { get; private set; }
    public AttemptStatus Status { get; private set; } = AttemptStatus.InProgress;
    public decimal? Score { get; private set; }
    public bool? IsPassed { get; private set; }
    public Guid? CertificateId { get; set; }

    private readonly List<IDomainEvent> _domainEvents = new();
    public IReadOnlyList<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    public User User { get; private set; } = default!;
    public Exam Exam { get; private set; } = default!;
    public Certificate? Certificate { get; private set; }
    public ICollection<AttemptAnswer> Answers { get; private set; } = new List<AttemptAnswer>();

    private ExamAttempt() { }

    public static ExamAttempt Start(Guid userId, Guid examId)
    {
        return new ExamAttempt
        {
            UserId = userId,
            ExamId = examId,
            StartedAt = DateTime.UtcNow
        };
    }

    public void Complete(decimal score, decimal passingPercentage)
    {
        Score = score;
        IsPassed = score >= passingPercentage;
        CompletedAt = DateTime.UtcNow;
        Status = AttemptStatus.Completed;
        UpdatedAt = DateTime.UtcNow;

        if (IsPassed == true)
        {
            _domainEvents.Add(new ExamPassedDomainEvent(UserId, ExamId, Id, score));
        }
    }

    public void TimeOut()
    {
        Status = AttemptStatus.TimedOut;
        CompletedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public void ClearDomainEvents() => _domainEvents.Clear();
}
