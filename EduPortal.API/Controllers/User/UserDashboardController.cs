using EduPortal.Application.Features.Dashboard;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EduPortal.API.Controllers.User;

[ApiController]
[Route("api/user/dashboard")]
[Authorize]
public class UserDashboardController : ControllerBase
{
    private readonly IMediator _mediator;

    public UserDashboardController(IMediator mediator) => _mediator = mediator;

    [HttpGet]
    public async Task<IActionResult> Get(CancellationToken ct)
    {
        var result = await _mediator.Send(new GetUserDashboardQuery(), ct);
        return Ok(result.Value);
    }
}
