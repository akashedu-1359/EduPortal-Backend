using EduPortal.Domain.Common;
using EduPortal.Domain.Enums;

namespace EduPortal.Domain.Entities;

public class Exam : BaseEntity
{
    public string Title { get; set; } = default!;
    public string Description { get; set; } = default!;
    public int DurationMinutes { get; set; }
    public decimal PassingPercentage { get; set; }
    public int MaxAttempts { get; set; }
    public DateTime? ScheduledStartAt { get; set; }
    public DateTime? ScheduledEndAt { get; set; }
    public ExamStatus Status { get; set; } = ExamStatus.Draft;
    public Guid CreatedByAdminId { get; set; }
    public bool IsDeleted { get; set; }

    public User CreatedByAdmin { get; private set; } = default!;
    public ICollection<Question> Questions { get; private set; } = new List<Question>();
    public ICollection<ExamAttempt> Attempts { get; private set; } = new List<ExamAttempt>();

    private Exam() { }

    public Exam(string title, string description, int durationMinutes, decimal passingPercentage, Guid adminId)
    {
        Title = title;
        Description = description;
        DurationMinutes = durationMinutes;
        PassingPercentage = passingPercentage;
        CreatedByAdminId = adminId;
    }
}
