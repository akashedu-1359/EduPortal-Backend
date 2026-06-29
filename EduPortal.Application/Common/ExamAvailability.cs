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
            return "This exam is not open yet.";

        if (exam.Status != ExamStatus.Active)
            return "Exam is not currently active.";

        if (exam.ScheduledStartAt.HasValue && exam.ScheduledStartAt.Value > utcNow)
            return "This exam is not open yet.";

        if (exam.ScheduledEndAt.HasValue && exam.ScheduledEndAt.Value <= utcNow)
            return "This exam has closed.";

        return null;
    }
}
