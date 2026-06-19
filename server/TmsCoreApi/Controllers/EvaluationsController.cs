using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using TmsCoreApi.Models;
using TmsCoreApi.Services;

namespace TmsCoreApi.Controllers;

[ApiController]
[Route("api/evaluations")]
public class EvaluationsController(IEvaluationService evaluationService) : ControllerBase
{
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(string id)
    {
        var evaluation = await evaluationService.GetByIdAsync(id);
        return evaluation is not null ? Ok(evaluation) : NotFound();
    }

    [HttpGet("bid/{bidId}")]
    public async Task<IActionResult> GetByBid(string bidId)
    {
        var evaluationSheets = await evaluationService.GetEvaluationsByBidIdAsync(bidId);
        return Ok(evaluationSheets);
    }

    [HttpPost]
    public async Task<IActionResult> Submit([FromBody] Evaluation evaluation)
    {
        var completedSheet = await evaluationService.SubmitEvaluationAsync(evaluation);
        return CreatedAtAction(nameof(GetById), new { id = completedSheet.Id }, completedSheet);
    }
}