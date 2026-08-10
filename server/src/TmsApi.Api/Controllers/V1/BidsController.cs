namespace TmsApi.Api.Controllers.V1;

using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TmsApi.Application.Bids.Commands.AddBidDocument;
using TmsApi.Application.Bids.Commands.SubmitBid;
using TmsApi.Application.Bids.DTOs;
using TmsApi.Application.Bids.Queries.GetBidsByTenderId;

[ApiController]
[Route("api/v1/bids")]
[Authorize]
public class BidsController : ControllerBase
{
    private readonly ISender _mediator;

    public BidsController(ISender mediator) => _mediator = mediator;

    [HttpPost]
    [Authorize(Roles = "Supplier")]
    public async Task<IActionResult> SubmitBid([FromBody] SubmitBidRequestDto dto, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new SubmitBidCommand(dto), cancellationToken);

        if (!result.IsSuccess)
            return BadRequest(new ProblemDetails { Detail = result.Error });

        return Ok(new { BidId = result.Value });
    }

    [HttpPost("documents")]
    [Authorize(Roles = "Supplier")]
    public async Task<IActionResult> AddDocument([FromBody] AddBidDocumentRequestDto dto, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new AddBidDocumentCommand(dto), cancellationToken);

        if (!result.IsSuccess)
            return BadRequest(new ProblemDetails { Detail = result.Error });

        return Ok(new { DocumentId = result.Value });
    }

    [HttpGet("tender/{tenderId:int}")]
    [Authorize(Roles = "TenderOfficer,Admin")]
    public async Task<IActionResult> GetByTenderId(int tenderId, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetBidsByTenderIdQuery(tenderId), cancellationToken);

        if (!result.IsSuccess)
            return NotFound(new ProblemDetails { Detail = result.Error });

        return Ok(result.Value);
    }
}