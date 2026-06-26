using EduPortal.Application.Common;
using EduPortal.Application.Interfaces;
using EduPortal.Domain.Enums;
using MediatR;

namespace EduPortal.Application.Features.Exams.Queries;

public record GetAttemptResultQuery(Guid AttemptId) : IRequest<Result<AttemptResultDto>>;

public record AttemptResultDto(
    Guid AttemptId, Guid ExamId, string ExamTitle, decimal Score, bool IsPassed,
    decimal PassingPercentage, int TotalQuestions, int CorrectAnswers,
    DateTime StartedAt, DateTime? CompletedAt, Guid? CertificateId,
    List<QuestionResultDto> QuestionResults);

public record QuestionResultDto(
    Guid QuestionId, string QuestionText, string Option1, string Option2,
    string Option3, string Option4, int? SelectedOptionIndex,
    int CorrectOptionIndex, bool IsCorrect);

public class GetAttemptResultQueryHandler : IRequestHandler<GetAttemptResultQuery, Result<AttemptResultDto>>
{
    private readonly IExamRepository _exams;
    private readonly ICurrentUserService _currentUser;

    public GetAttemptResultQueryHandler(IExamRepository exams, ICurrentUserService currentUser)
    { _exams = exams; _currentUser = currentUser; }

    public async Task<Result<AttemptResultDto>> Handle(GetAttemptResultQuery request, CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId ?? Guid.Empty;

        var attempt = await _exams.GetAttemptAsync(request.AttemptId, cancellationToken);
        if (attempt == null) return Result<AttemptResultDto>.NotFound("Attempt not found.");
        if (attempt.UserId != userId) return Result<AttemptResultDto>.Unauthorized();
        if (attempt.Status != AttemptStatus.Completed && attempt.Status != AttemptStatus.TimedOut)
            return Result<AttemptResultDto>.Failure("Attempt is still in progress.", 400);

        var exam = await _exams.GetByIdAsync(attempt.ExamId, includeQuestions: true, ct: cancellationToken);
        if (exam == null) return Result<AttemptResultDto>.NotFound("Exam not found.");

        var answerMap = attempt.Answers.ToDictionary(a => a.QuestionId);
        var questionResults = exam.Questions.OrderBy(q => q.SortOrder).Select(q =>
        {
            answerMap.TryGetValue(q.Id, out var answer);
            return new QuestionResultDto(
                q.Id, q.QuestionText, q.Option1, q.Option2, q.Option3, q.Option4,
                answer?.SelectedOptionIndex, q.CorrectOptionIndex, answer?.IsCorrect ?? false);
        }).ToList();

        var correctCount = questionResults.Count(qr => qr.IsCorrect);

        return Result<AttemptResultDto>.Success(new AttemptResultDto(
            attempt.Id, attempt.ExamId, exam.Title,
            attempt.Score ?? 0, attempt.IsPassed ?? false,
            exam.PassingPercentage, exam.Questions.Count, correctCount,
            attempt.StartedAt, attempt.CompletedAt, attempt.CertificateId,
            questionResults));
    }
}
