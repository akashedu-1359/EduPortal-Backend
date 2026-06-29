using EduPortal.Application.Common;
using EduPortal.Application.Interfaces;
using EduPortal.Domain.Entities;
using EduPortal.Domain.Enums;
using MediatR;

namespace EduPortal.Application.Features.Exams.Commands;

public record BulkQuestionItem(
    string QuestionText,
    string Option1,
    string Option2,
    string Option3,
    string Option4,
    int CorrectOptionIndex,
    int SortOrder = 0);

public record BulkAddQuestionsCommand(Guid ExamId, List<BulkQuestionItem> Questions)
    : IRequest<Result<BulkAddQuestionsResult>>;

public record BulkAddQuestionsResult(int AddedCount);

public class BulkAddQuestionsCommandHandler : IRequestHandler<BulkAddQuestionsCommand, Result<BulkAddQuestionsResult>>
{
    private const int MaxBatchSize = 200;
    private readonly IExamRepository _exams;

    public BulkAddQuestionsCommandHandler(IExamRepository exams) => _exams = exams;

    public async Task<Result<BulkAddQuestionsResult>> Handle(
        BulkAddQuestionsCommand request,
        CancellationToken cancellationToken)
    {
        var status = await _exams.GetExamStatusAsync(request.ExamId, cancellationToken);
        if (status == null) return Result<BulkAddQuestionsResult>.NotFound("Exam not found.");
        if (status == ExamStatus.Active)
            return Result<BulkAddQuestionsResult>.Failure("Cannot add questions to an active exam.", 400);
        if (request.Questions.Count == 0)
            return Result<BulkAddQuestionsResult>.Failure("No questions provided.", 400);
        if (request.Questions.Count > MaxBatchSize)
            return Result<BulkAddQuestionsResult>.Failure($"Maximum {MaxBatchSize} questions per upload.", 400);

        var baseCount = await _exams.GetQuestionCountAsync(request.ExamId, cancellationToken);
        var nextSort = baseCount;

        foreach (var item in request.Questions)
        {
            nextSort++;
            var sortOrder = item.SortOrder > 0 ? item.SortOrder : nextSort;
            var question = new Question(
                request.ExamId,
                item.QuestionText,
                item.Option1,
                item.Option2,
                item.Option3,
                item.Option4,
                item.CorrectOptionIndex,
                sortOrder);

            await _exams.AddQuestionAsync(question, cancellationToken);
        }

        await _exams.SaveChangesAsync(cancellationToken);
        return Result<BulkAddQuestionsResult>.Success(new BulkAddQuestionsResult(request.Questions.Count));
    }
}
