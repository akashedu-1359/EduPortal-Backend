using EduPortal.Domain.Common;

namespace EduPortal.Domain.Entities;

public class AttemptAnswer : BaseEntity
{
    public Guid ExamAttemptId { get; private set; }
    public Guid QuestionId { get; private set; }
    public int? SelectedOptionIndex { get; private set; }
    public bool IsCorrect { get; private set; }

    public ExamAttempt ExamAttempt { get; private set; } = default!;
    public Question Question { get; private set; } = default!;

    private AttemptAnswer() { }

    public static AttemptAnswer Create(Guid attemptId, Guid questionId, int? selectedOption, int correctOption)
    {
        return new AttemptAnswer
        {
            ExamAttemptId = attemptId,
            QuestionId = questionId,
            SelectedOptionIndex = selectedOption,
            IsCorrect = selectedOption.HasValue && selectedOption.Value == correctOption
        };
    }
}
