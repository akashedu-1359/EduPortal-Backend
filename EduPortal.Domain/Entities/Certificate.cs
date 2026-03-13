using EduPortal.Domain.Common;

namespace EduPortal.Domain.Entities;

public class Certificate : BaseEntity
{
    public Guid UserId { get; private set; }
    public Guid ExamAttemptId { get; private set; }
    public string StorageKey { get; private set; } = default!;
    public DateTime IssuedAt { get; private set; }

    public User User { get; private set; } = default!;
    public ExamAttempt ExamAttempt { get; private set; } = default!;

    private Certificate() { }

    public static Certificate Create(Guid userId, Guid attemptId, string storageKey)
    {
        return new Certificate
        {
            UserId = userId,
            ExamAttemptId = attemptId,
            StorageKey = storageKey,
            IssuedAt = DateTime.UtcNow
        };
    }
}
