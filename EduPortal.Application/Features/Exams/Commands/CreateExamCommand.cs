using EduPortal.Application.Common;
using EduPortal.Application.Interfaces;
using EduPortal.Domain.Entities;
using MediatR;

namespace EduPortal.Application.Features.Exams.Commands;

public record QuestionDto(string QuestionText, string Option1, string Option2, string Option3, string Option4, int CorrectOptionIndex, int SortOrder);

public record CreateExamCommand(
    string Title,
    string Description,
    int DurationMinutes,
    decimal PassingPercentage,
    int MaxAttempts,
    DateTime? ScheduledStartAt,
    DateTime? ScheduledEndAt,
    List<QuestionDto> Questions) : IRequest<Result<Guid>>;

public class CreateExamCommandHandler : IRequestHandler<CreateExamCommand, Result<Guid>>
{
    private readonly IExamRepository _exams;
    private readonly ICurrentUserService _currentUser;

    public CreateExamCommandHandler(IExamRepository exams, ICurrentUserService currentUser)
    { _exams = exams; _currentUser = currentUser; }

    public async Task<Result<Guid>> Handle(CreateExamCommand request, CancellationToken cancellationToken)
    {
        var adminId = _currentUser.UserId ?? Guid.Empty;
        var exam = new Exam(request.Title, request.Description, request.DurationMinutes, request.PassingPercentage, adminId)
        {
            MaxAttempts = request.MaxAttempts,
            ScheduledStartAt = request.ScheduledStartAt,
            ScheduledEndAt = request.ScheduledEndAt
        };

        foreach (var q in request.Questions)
            exam.Questions.Add(new Question(exam.Id, q.QuestionText, q.Option1, q.Option2, q.Option3, q.Option4, q.CorrectOptionIndex, q.SortOrder));

        await _exams.AddAsync(exam, cancellationToken);
        await _exams.SaveChangesAsync(cancellationToken);
        return Result<Guid>.Created(exam.Id);
    }
}
