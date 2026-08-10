namespace TmsApi.Api.Controllers.V1;

using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TmsApi.Application.Common.Models;
using TmsApi.Application.Tenders.DTOs;
using TmsApi.Application.Tenders.QueriesAndCommands;
using TmsApi.Domain.Entities;

[ApiController]
[Route("api/v1/tenders")]
public class TendersController : ControllerBase
{
    private readonly ISender _mediator;
    private readonly LinkGenerator _linkGenerator;

    public TendersController(ISender mediator, LinkGenerator linkGenerator)
    {
        _mediator = mediator;
        _linkGenerator = linkGenerator;
    }

    [HttpPost]
    [Authorize(Roles = "TenderOfficer,Admin")]
    [EndpointSummary("Create a new tender draft")]
    [ProducesResponseType(typeof(TenderResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateTenderRequestDto request, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new CreateTenderCommand(request), cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpGet]
    [EndpointSummary("Get paged and filtered list of tenders")]
    [ProducesResponseType(typeof(PagedResponse<TenderResponseDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(
        [FromQuery] TenderStatus? status,
        [FromQuery] PaginationParams pagination,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetPagedTendersQuery(status, pagination), cancellationToken);
        return Ok(result);
    }

    [HttpGet("{id:int}")]
    [EndpointSummary("Get detailed tender by ID with hypermedia links")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id, CancellationToken cancellationToken)
    {
        var tender = await _mediator.Send(new GetTenderByIdQuery(id), cancellationToken);
        if (tender is null) return NotFound();

        // HATEOAS Links Generation
        var links = new List<object>
        {
            new { rel = "self", method = "GET", href = _linkGenerator.GetPathByAction(HttpContext, nameof(GetById), values: new { id }) },
            new { rel = "upload-document", method = "POST", href = $"/api/v1/tenders/{id}/documents" }
        };

        if (tender.Status == TenderStatus.Draft.ToString())
        {
            links.Add(new { rel = "publish", method = "PATCH", href = $"/api/v1/tenders/{id}/publish" });
        }
        else if (tender.Status == TenderStatus.Published.ToString())
        {
            links.Add(new { rel = "close", method = "PATCH", href = $"/api/v1/tenders/{id}/close" });
            links.Add(new { rel = "submit-bid", method = "POST", href = $"/api/v1/tenders/{id}/bids" });
        }

        return Ok(new { data = tender, links });
    }

    [HttpPatch("{id:int}/publish")]
    [Authorize(Roles = "TenderOfficer,Admin")]
    [EndpointSummary("Publish a draft tender")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Publish(int id, CancellationToken cancellationToken)
    {
        var success = await _mediator.Send(new PublishTenderCommand(id), cancellationToken);
        return success ? NoContent() : BadRequest(new ProblemDetails { Detail = "Unable to publish tender. Ensure it exists and is currently in Draft status." });
    }

    [HttpPatch("{id:int}/close")]
    [Authorize(Roles = "TenderOfficer,Admin")]
    [EndpointSummary("Close a published tender")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Close(int id, CancellationToken cancellationToken)
    {
        var success = await _mediator.Send(new CloseTenderCommand(id), cancellationToken);
        return success ? NoContent() : BadRequest(new ProblemDetails { Detail = "Unable to close tender. Ensure it exists and is currently Published." });
    }

    [HttpPost("{id:int}/award")]
    [Authorize(Roles = "TenderOfficer,Admin")]
    [EndpointSummary("Award a tender to a specific winning bid")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Award(int id, [FromQuery] int bidId, CancellationToken cancellationToken)
    {
        var success = await _mediator.Send(new AwardTenderCommand(id, bidId), cancellationToken);
        return success ? NoContent() : BadRequest(new ProblemDetails { Detail = "Unable to award tender. Check tender status or bid validity." });
    }
}