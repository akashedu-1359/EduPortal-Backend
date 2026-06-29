using EduPortal.Application.Features.Enrollments.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EduPortal.API.Controllers.User;

[ApiController]
[Route("api/user/enrollments")]
[Authorize]
public class UserEnrollmentsController : ControllerBase
{
    private readonly IMediator _mediator;

    public UserEnrollmentsController(IMediator mediator) => _mediator = mediator;

    [HttpGet]
    public async Task<IActionResult> GetMyEnrollments(CancellationToken ct)
    {
        var result = await _mediator.Send(new GetUserEnrollmentsQuery(), ct);
        return Ok(new { success = true, data = result.Value });
    }
}
