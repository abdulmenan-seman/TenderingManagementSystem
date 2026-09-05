namespace TmsApi.Api.Controllers.V1;

using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TmsApi.Application.Evaluations.Commands.FinalizeTenderResult;
using TmsApi.Application.Evaluations.Commands.SubmitBidEvaluation;
using TmsApi.Application.Evaluations.DTOs;
using TmsApi.Application.Evaluations.Queries.GetEvaluationsByBidId;

[ApiController]
[Route("api/v1/bid-evaluations")]
[Authorize(Roles = "TenderOfficer,Evaluator,Admin")]
public class BidEvaluationsController : ControllerBase
{
    private readonly ISender _mediator;

    public BidEvaluationsController(ISender mediator) => _mediator = mediator;

    [HttpPost("score")]
    public async Task<IActionResult> SubmitEvaluation([FromBody] SubmitBidEvaluationRequestDto dto, CancellationToken cancellationToken)
    {
        if (!int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var evaluatorId))
            return Unauthorized();

        var result = await _mediator.Send(new SubmitBidEvaluationCommand(evaluatorId, dto), cancellationToken);

        if (!result.IsSuccess)
            return BadRequest(new ProblemDetails { Detail = result.Error });

        return Ok(new { EvaluationId = result.Value });
    }

    [HttpGet("bid/{bidId:int}")]
    public async Task<IActionResult> GetByBidId(int bidId, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetEvaluationsByBidIdQuery(bidId), cancellationToken);

        if (!result.IsSuccess)
            return BadRequest(new ProblemDetails { Detail = result.Error });

        return Ok(result.Value);
    }

    [HttpPost("finalize-result")]
    [Authorize(Roles = "TenderOfficer,Admin")]
    public async Task<IActionResult> FinalizeResult([FromBody] FinalizeTenderResultRequestDto dto, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new FinalizeTenderResultCommand(dto), cancellationToken);

        if (!result.IsSuccess)
            return BadRequest(new ProblemDetails { Detail = result.Error });

        return Ok(result.Value);
    }
}