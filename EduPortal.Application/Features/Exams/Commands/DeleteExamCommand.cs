using EduPortal.Application.Common;
using EduPortal.Application.Interfaces;
using MediatR;

namespace EduPortal.Application.Features.Exams.Commands;

public record DeleteExamCommand(Guid Id) : IRequest<Result>;

public class DeleteExamCommandHandler : IRequestHandler<DeleteExamCommand, Result>
{
    private readonly IExamRepository _exams;

    public DeleteExamCommandHandler(IExamRepository exams) => _exams = exams;

    public async Task<Result> Handle(DeleteExamCommand request, CancellationToken cancellationToken)
    {
        var exam = await _exams.GetByIdAsync(request.Id, ct: cancellationToken);
        if (exam == null) return Result.NotFound("Exam not found.");

        exam.IsDeleted = true;
        await _exams.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
