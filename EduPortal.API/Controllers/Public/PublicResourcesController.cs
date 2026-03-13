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
    public PublicResourcesController(IMediator mediator) => _mediator = mediator;

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] int page = 1, [FromQuery] int pageSize = 20,
        [FromQuery] ResourceType? type = null, [FromQuery] Guid? categoryId = null,
        [FromQuery] bool? featured = null, [FromQuery] string? search = null, CancellationToken ct = default)
    {
        var result = await _mediator.Send(new GetPublicResourcesQuery(page, pageSize, type, categoryId, featured, search), ct);
        return Ok(new { success = true, data = result.Value });
    }
}
