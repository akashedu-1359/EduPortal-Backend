using EduPortal.Domain.Common;

namespace EduPortal.Domain.Entities;

public class Question : BaseEntity
{
    public Guid ExamId { get; private set; }
    public string QuestionText { get; set; } = default!;
    public string Option1 { get; set; } = default!;
    public string Option2 { get; set; } = default!;
    public string Option3 { get; set; } = default!;
    public string Option4 { get; set; } = default!;
    public int CorrectOptionIndex { get; private set; }
    public int SortOrder { get; set; }

    public Exam Exam { get; private set; } = default!;
    public ICollection<AttemptAnswer> AttemptAnswers { get; private set; } = new List<AttemptAnswer>();

    private Question() { }

    public Question(Guid examId, string questionText, string opt1, string opt2, string opt3, string opt4, int correctIndex, int sortOrder)
    {
        ExamId = examId;
        QuestionText = questionText;
        Option1 = opt1;
        Option2 = opt2;
        Option3 = opt3;
        Option4 = opt4;
        CorrectOptionIndex = correctIndex;
        SortOrder = sortOrder;
    }

    public void UpdateCorrectOption(int index) => CorrectOptionIndex = index;
}
