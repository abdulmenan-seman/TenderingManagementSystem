namespace TmsApi.Api.Controllers.V1;

using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TmsApi.Application.Criteria.Commands.AddEvaluationCriteria;
using TmsApi.Application.Criteria.Commands.DeleteEvaluationCriteria;
using TmsApi.Application.Criteria.DTOs;
using TmsApi.Application.Criteria.Queries.GetCriteriaByTenderId;

[ApiController]
[Route("api/v1/evaluation-criteria")]
[Authorize]
public class EvaluationCriteriaController : ControllerBase
{
    private readonly ISender _mediator;

    public EvaluationCriteriaController(ISender mediator) => _mediator = mediator;

    [HttpPost]
    [Authorize(Roles = "TenderOfficer,Admin")]
    public async Task<IActionResult> Create([FromBody] AddEvaluationCriteriaRequestDto dto, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(dto.CriteriaName))
            return BadRequest(new ProblemDetails { Detail = "Criterion name is required." });

        if (string.IsNullOrWhiteSpace(dto.Description))
            return BadRequest(new ProblemDetails { Detail = "Criterion description is required." });

        var result = await _mediator.Send(new AddEvaluationCriteriaCommand(dto), cancellationToken);

        if (!result.IsSuccess)
            return BadRequest(new ProblemDetails { Detail = result.Error });

        return CreatedAtAction(nameof(GetById), new { id = result.Value }, new { Id = result.Value });
    }

    [HttpGet("{id:int}")]
    [Authorize(Roles = "TenderOfficer,Evaluator,Admin")]
    public async Task<IActionResult> GetById(int id, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetCriteriaByIdQuery(id), cancellationToken);

        if (!result.IsSuccess)
            return NotFound(new ProblemDetails { Detail = result.Error });

        return Ok(result.Value);
    }

    [HttpGet("tender/{tenderId:int}")]
    [Authorize(Roles = "TenderOfficer,Evaluator,Admin")]
    public async Task<IActionResult> GetByTenderId(int tenderId, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetCriteriaByTenderIdQuery(tenderId), cancellationToken);

        if (!result.IsSuccess)
            return BadRequest(new ProblemDetails { Detail = result.Error });

        return Ok(result.Value);
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = "TenderOfficer,Admin")]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new DeleteEvaluationCriteriaCommand(id), cancellationToken);

        if (!result.IsSuccess)
            return NotFound(new ProblemDetails { Detail = result.Error });

        return NoContent();
    }
}