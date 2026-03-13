using EduPortal.Application.Features.Users.Commands;
using EduPortal.Application.Features.Users.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EduPortal.API.Controllers.Admin;

[ApiController]
[Route("api/admin/users")]
[Authorize(Policy = "UserManage")]
public class AdminUsersController : ControllerBase
{
    private readonly IMediator _mediator;

    public AdminUsersController(IMediator mediator) => _mediator = mediator;

    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? search = null,
        CancellationToken ct = default)
    {
        var result = await _mediator.Send(new GetUsersQuery(page, pageSize, search), ct);
        return Ok(result.Value);
    }

    [HttpPatch("{id:guid}/status")]
    public async Task<IActionResult> UpdateStatus(Guid id, [FromBody] UpdateStatusRequest req, CancellationToken ct)
    {
        var result = await _mediator.Send(new UpdateUserStatusCommand(id, req.IsActive), ct);
        return result.IsSuccess ? NoContent() : StatusCode(result.StatusCode, new { error = result.Error });
    }
}

public record UpdateStatusRequest(bool IsActive);
