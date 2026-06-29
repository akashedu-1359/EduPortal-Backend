using EduPortal.Application.Features.Exams.Commands;
using EduPortal.Application.Features.Exams.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EduPortal.API.Controllers.User;

[ApiController]
[Route("api/user/exams")]
[Authorize]
public class UserExamsController : ControllerBase
{
    private readonly IMediator _mediator;

    public UserExamsController(IMediator mediator) => _mediator = mediator;

    [HttpGet]
    public async Task<IActionResult> GetPublished([FromQuery] int page = 1, [FromQuery] int pageSize = 20, CancellationToken ct = default)
    {
        var result = await _mediator.Send(new GetExamsQuery(page, pageSize, ActiveOnly: true), ct);
        return Ok(new { success = true, data = result.Value });
    }

    [HttpGet("{examId:guid}")]
    public async Task<IActionResult> GetDetail(Guid examId, CancellationToken ct)
    {
        var result = await _mediator.Send(new GetExamByIdForUserQuery(examId), ct);
        return result.IsSuccess
            ? Ok(new { success = true, data = result.Value })
            : StatusCode(result.StatusCode, new { success = false, error = result.Error });
    }

    [HttpGet("attempts")]
    public async Task<IActionResult> GetMyAttempts([FromQuery] int page = 1, [FromQuery] int pageSize = 20, CancellationToken ct = default)
    {
        var result = await _mediator.Send(new GetUserAttemptsQuery(page, pageSize), ct);
        return result.IsSuccess ? Ok(result.Value) : StatusCode(result.StatusCode, new { error = result.Error });
    }

    [HttpGet("attempts/{attemptId:guid}/result")]
    public async Task<IActionResult> GetAttemptResult(Guid attemptId, CancellationToken ct)
    {
        var result = await _mediator.Send(new GetAttemptResultQuery(attemptId), ct);
        return result.IsSuccess ? Ok(result.Value) : StatusCode(result.StatusCode, new { error = result.Error });
    }

    [HttpPost("{examId:guid}/start")]
    public async Task<IActionResult> Start(Guid examId, CancellationToken ct)
    {
        var result = await _mediator.Send(new StartExamAttemptCommand(examId), ct);
        return result.IsSuccess ? StatusCode(201, result.Value) : StatusCode(result.StatusCode, new { error = result.Error });
    }

    [HttpPost("attempts/{attemptId:guid}/submit")]
    public async Task<IActionResult> Submit(Guid attemptId, [FromBody] SubmitExamCommand command, CancellationToken ct)
    {
        if (attemptId != command.AttemptId) return BadRequest(new { error = "AttemptId mismatch." });
        var result = await _mediator.Send(command, ct);
        return result.IsSuccess ? Ok(result.Value) : StatusCode(result.StatusCode, new { error = result.Error });
    }
}
