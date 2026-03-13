using EduPortal.Application.Features.Enrollments.Commands;
using EduPortal.Application.Features.Enrollments.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EduPortal.API.Controllers.User;

[ApiController]
[Route("api/user/resources")]
[Authorize]
public class UserResourcesController : ControllerBase
{
    private readonly IMediator _mediator;

    public UserResourcesController(IMediator mediator) => _mediator = mediator;

    [HttpPost("{resourceId:guid}/enroll")]
    public async Task<IActionResult> EnrollFree(Guid resourceId, CancellationToken ct)
    {
        var result = await _mediator.Send(new EnrollFreeCommand(resourceId), ct);
        return result.IsSuccess ? Ok(new { message = "Enrolled successfully." }) : StatusCode(result.StatusCode, new { error = result.Error });
    }

    [HttpGet("{resourceId:guid}/access")]
    public async Task<IActionResult> GetAccess(Guid resourceId, CancellationToken ct)
    {
        var result = await _mediator.Send(new GetResourceAccessQuery(resourceId), ct);
        return result.IsSuccess ? Ok(result.Value) : StatusCode(result.StatusCode, new { error = result.Error });
    }
}
