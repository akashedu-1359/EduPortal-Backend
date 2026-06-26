namespace EduPortal.Domain.Enums;

public enum ExamStatus
{
    /// <summary>Default state. Editable.</summary>
    Draft,
    /// <summary>Reserved for future use.</summary>
    Published,
    /// <summary>Reserved for future use.</summary>
    Scheduled,
    /// <summary>Live and takeable by students. Set by Publish command or scheduler.</summary>
    Active,
    /// <summary>Reserved for future use.</summary>
    Closed,
    /// <summary>Reserved for future use.</summary>
    Archived,
    /// <summary>Auto-set by scheduler when ScheduledEndAt passes.</summary>
    Completed
}
