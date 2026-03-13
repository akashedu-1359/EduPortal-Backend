using EduPortal.Application.Features.Exams.Commands;
using EduPortal.Application.Features.Exams.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EduPortal.API.Controllers.Admin;

[ApiController]
[Route("api/admin/exams")]
[Authorize(Policy = "ExamManage")]
public class AdminExamsController : ControllerBase
{
    private readonly IMediator _mediator;

    public AdminExamsController(IMediator mediator) => _mediator = mediator;

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] int page = 1, [FromQuery] int pageSize = 20, CancellationToken ct = default)
    {
        var result = await _mediator.Send(new GetExamsQuery(page, pageSize), ct);
        return Ok(result.Value);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var result = await _mediator.Send(new GetExamDetailQuery(id, IsAdmin: true), ct);
        return result.IsSuccess ? Ok(result.Value) : StatusCode(result.StatusCode, new { error = result.Error });
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateExamCommand command, CancellationToken ct)
    {
        var result = await _mediator.Send(command, ct);
        return result.IsSuccess ? StatusCode(201, new { id = result.Value }) : StatusCode(result.StatusCode, new { error = result.Error });
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateExamCommand command, CancellationToken ct)
    {
        if (id != command.Id) return BadRequest(new { error = "ID mismatch." });
        var result = await _mediator.Send(command, ct);
        return result.IsSuccess ? NoContent() : StatusCode(result.StatusCode, new { error = result.Error });
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        var result = await _mediator.Send(new DeleteExamCommand(id), ct);
        return result.IsSuccess ? NoContent() : StatusCode(result.StatusCode, new { error = result.Error });
    }

    [HttpPost("{id:guid}/publish")]
    public async Task<IActionResult> Publish(Guid id, CancellationToken ct)
    {
        var result = await _mediator.Send(new PublishExamCommand(id), ct);
        return result.IsSuccess ? Ok(new { message = "Exam published." }) : StatusCode(result.StatusCode, new { error = result.Error });
    }

    [HttpPost("{id:guid}/unpublish")]
    public async Task<IActionResult> Unpublish(Guid id, CancellationToken ct)
    {
        var result = await _mediator.Send(new UnpublishExamCommand(id), ct);
        return result.IsSuccess ? Ok(new { message = "Exam unpublished." }) : StatusCode(result.StatusCode, new { error = result.Error });
    }
}
