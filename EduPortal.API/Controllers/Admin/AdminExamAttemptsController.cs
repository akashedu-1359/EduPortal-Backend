using EduPortal.Application.Features.Exams.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EduPortal.API.Controllers.Admin;

[ApiController]
[Route("api/admin/exam-attempts")]
[Authorize(Policy = "ExamManage")]
public class AdminExamAttemptsController : ControllerBase
{
    private readonly IMediator _mediator;

    public AdminExamAttemptsController(IMediator mediator) => _mediator = mediator;

    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] Guid? examId = null,
        CancellationToken ct = default)
    {
        var result = await _mediator.Send(new GetAdminExamAttemptsQuery(page, pageSize, examId), ct);
        return Ok(new { success = true, data = result.Value });
    }
}
