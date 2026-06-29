using EduPortal.Domain.Entities;
using EduPortal.Domain.Enums;

namespace EduPortal.Application.Common;

public static class ExamAvailability
{
    public static bool IsTakeable(Exam exam, DateTime utcNow)
    {
        if (exam.Status != ExamStatus.Active) return false;
        if (exam.ScheduledStartAt.HasValue && exam.ScheduledStartAt.Value > utcNow) return false;
        if (exam.ScheduledEndAt.HasValue && exam.ScheduledEndAt.Value <= utcNow) return false;
        return true;
    }

    public static string? GetUnavailableMessage(Exam exam, DateTime utcNow)
    {
        if (exam.Status == ExamStatus.Scheduled)
            return exam.ScheduledStartAt.HasValue
                ? $"This exam opens on {exam.ScheduledStartAt.Value:u} UTC."
                : "This exam is scheduled but not yet open.";

        if (exam.Status != ExamStatus.Active)
            return "Exam is not currently active.";

        if (exam.ScheduledStartAt.HasValue && exam.ScheduledStartAt.Value > utcNow)
            return $"This exam opens on {exam.ScheduledStartAt.Value:u} UTC.";

        if (exam.ScheduledEndAt.HasValue && exam.ScheduledEndAt.Value <= utcNow)
            return "This exam has closed.";

        return null;
    }
}
