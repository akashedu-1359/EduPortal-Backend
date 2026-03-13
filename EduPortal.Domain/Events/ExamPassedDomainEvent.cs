namespace EduPortal.Domain.Events;

public record ExamPassedDomainEvent(
    Guid UserId,
    Guid ExamId,
    Guid AttemptId,
    decimal Score) : IDomainEvent;
