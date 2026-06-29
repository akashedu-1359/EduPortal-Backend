namespace EduPortal.Application.Interfaces;

public record QuestionScoringInfo(Guid Id, int CorrectOptionIndex);

public record ExamScoringInfo(
    int DurationMinutes,
    decimal PassingPercentage,
    IReadOnlyList<QuestionScoringInfo> Questions);
