namespace EduPortal.Domain.Enums;

public enum AttemptStatus
{
    /// <summary>Student is actively taking the exam.</summary>
    InProgress,
    /// <summary>Student submitted or grading completed.</summary>
    Completed,
    /// <summary>Server-side timeout enforcement.</summary>
    TimedOut,
    /// <summary>Reserved for future use.</summary>
    Abandoned
}
