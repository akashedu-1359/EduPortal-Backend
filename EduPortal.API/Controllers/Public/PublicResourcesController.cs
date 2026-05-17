using EduPortal.Application.Common;
using EduPortal.Application.Features.Resources.Queries;
using EduPortal.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace EduPortal.API.Controllers.Public;

[ApiController]
[Route("api/resources")]
public class PublicResourcesController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ICurrentUserService _currentUser;

    public PublicResourcesController(IMediator mediator, ICurrentUserService currentUser)
    {
        _mediator = mediator;
        _currentUser = currentUser;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 20,
        [FromQuery] ResourceType? type = null, [FromQuery] Guid? categoryId = null,
        [FromQuery] bool? featured = null, [FromQuery] string? search = null, CancellationToken ct = default)
    {
        var result = await _mediator.Send(new GetPublicResourcesQuery(pageNumber, pageSize, type, categoryId, featured, search), ct);
        return Ok(new { success = true, data = result.Value });
    }

    [HttpGet("{slug}")]
    public async Task<IActionResult> GetBySlug(string slug, CancellationToken ct)
    {
        var result = await _mediator.Send(new GetPublicResourceBySlugQuery(slug, _currentUser.UserId), ct);
        return result.IsSuccess ? Ok(new { success = true, data = result.Value })
            : StatusCode(result.StatusCode, new { success = false, error = result.Error });
    }
}
