using EduPortal.Application.Features.Categories.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace EduPortal.API.Controllers.Public;

[ApiController]
[Route("api/categories")]
public class PublicCategoriesController : ControllerBase
{
    private readonly IMediator _mediator;
    public PublicCategoriesController(IMediator mediator) => _mediator = mediator;

    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken ct)
    {
        var result = await _mediator.Send(new GetPublicCategoriesQuery(), ct);
        return Ok(new { success = true, data = result.Value });
    }
}
