using EduPortal.Application.Common;
using EduPortal.Application.Interfaces;
using MediatR;

namespace EduPortal.Application.Features.Exams.Queries;

public record GetExamQuestionsQuery(Guid ExamId) : IRequest<Result<List<QuestionDetailDto>>>;

public class GetExamQuestionsQueryHandler : IRequestHandler<GetExamQuestionsQuery, Result<List<QuestionDetailDto>>>
{
    private readonly IExamRepository _exams;

    public GetExamQuestionsQueryHandler(IExamRepository exams) => _exams = exams;

    public async Task<Result<List<QuestionDetailDto>>> Handle(GetExamQuestionsQuery request, CancellationToken cancellationToken)
    {
        var exam = await _exams.GetByIdAsync(request.ExamId, includeQuestions: true, ct: cancellationToken);
        if (exam == null) return Result<List<QuestionDetailDto>>.NotFound("Exam not found.");

        var dtos = exam.Questions.OrderBy(q => q.SortOrder)
            .Select(q => new QuestionDetailDto(
                q.Id, q.QuestionText, q.Option1, q.Option2, q.Option3, q.Option4,
                q.CorrectOptionIndex, q.SortOrder))
            .ToList();

        return Result<List<QuestionDetailDto>>.Success(dtos);
    }
}
