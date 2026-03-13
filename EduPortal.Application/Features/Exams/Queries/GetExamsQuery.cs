using EduPortal.Application.Common;
using EduPortal.Application.Interfaces;
using MediatR;

namespace EduPortal.Application.Features.Exams.Queries;

public record GetExamsQuery(int Page = 1, int PageSize = 20) : IRequest<Result<PagedResult<ExamSummaryDto>>>;

public record ExamSummaryDto(
    Guid Id,
    string Title,
    string Description,
    int DurationMinutes,
    decimal PassingPercentage,
    int MaxAttempts,
    string Status,
    DateTime? ScheduledStartAt,
    DateTime? ScheduledEndAt,
    int QuestionCount,
    DateTime CreatedAt);

public record GetExamDetailQuery(Guid Id, bool IsAdmin = false) : IRequest<Result<ExamDetailDto>>;

public record ExamDetailDto(
    Guid Id,
    string Title,
    string Description,
    int DurationMinutes,
    decimal PassingPercentage,
    int MaxAttempts,
    string Status,
    DateTime? ScheduledStartAt,
    DateTime? ScheduledEndAt,
    List<QuestionDetailDto> Questions,
    DateTime CreatedAt);

public record QuestionDetailDto(
    Guid Id,
    string QuestionText,
    string Option1,
    string Option2,
    string Option3,
    string Option4,
    int? CorrectOptionIndex,
    int SortOrder);

public class GetExamsQueryHandler : IRequestHandler<GetExamsQuery, Result<PagedResult<ExamSummaryDto>>>
{
    private readonly IExamRepository _exams;
    public GetExamsQueryHandler(IExamRepository exams) => _exams = exams;

    public async Task<Result<PagedResult<ExamSummaryDto>>> Handle(GetExamsQuery request, CancellationToken cancellationToken)
    {
        var (items, total) = await _exams.GetPagedAsync(request.Page, request.PageSize, cancellationToken);
        var dtos = items.Select(e => new ExamSummaryDto(
            e.Id, e.Title, e.Description, e.DurationMinutes,
            e.PassingPercentage, e.MaxAttempts, e.Status.ToString(),
            e.ScheduledStartAt, e.ScheduledEndAt, e.Questions.Count, e.CreatedAt)).ToList();
        return Result<PagedResult<ExamSummaryDto>>.Success(PagedResult<ExamSummaryDto>.Create(dtos, request.Page, request.PageSize, total));
    }
}

public class GetExamDetailQueryHandler : IRequestHandler<GetExamDetailQuery, Result<ExamDetailDto>>
{
    private readonly IExamRepository _exams;
    public GetExamDetailQueryHandler(IExamRepository exams) => _exams = exams;

    public async Task<Result<ExamDetailDto>> Handle(GetExamDetailQuery request, CancellationToken cancellationToken)
    {
        var exam = await _exams.GetByIdAsync(request.Id, includeQuestions: true, ct: cancellationToken);
        if (exam == null) return Result<ExamDetailDto>.NotFound("Exam not found.");

        var qs = exam.Questions.OrderBy(q => q.SortOrder)
            .Select(q => new QuestionDetailDto(
                q.Id, q.QuestionText, q.Option1, q.Option2, q.Option3, q.Option4,
                request.IsAdmin ? q.CorrectOptionIndex : null,
                q.SortOrder))
            .ToList();

        return Result<ExamDetailDto>.Success(new ExamDetailDto(
            exam.Id, exam.Title, exam.Description, exam.DurationMinutes,
            exam.PassingPercentage, exam.MaxAttempts, exam.Status.ToString(),
            exam.ScheduledStartAt, exam.ScheduledEndAt, qs, exam.CreatedAt));
    }
}
