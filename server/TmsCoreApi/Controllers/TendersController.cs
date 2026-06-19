using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using TmsCoreApi.Models;
using TmsCoreApi.Services;

namespace TmsCoreApi.Controllers;

[ApiController]
[Route("api/tenders")]
public class TendersController(ITenderService tenderService) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var tenders = await tenderService.GetAllAsync();
        return Ok(tenders);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(string id)
    {
        var tender = await tenderService.GetByIdAsync(id);
        return tender is not null ? Ok(tender) : NotFound();
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] Tender tender)
    {
        var createdTender = await tenderService.CreateAsync(tender);
        return CreatedAtAction(nameof(GetById), new { id = createdTender.Id }, createdTender);
    }

    [HttpPatch("{id}/status")]
    public async Task<IActionResult> UpdateStatus(string id, [FromBody] TenderStatus status)
    {
        var updated = await tenderService.UpdateStatusAsync(id, status);
        return updated ? NoContent() : NotFound();
    }

    [HttpPost("{id}/award")]
    public async Task<IActionResult> AwardTender(string id, [FromBody] string winningSupplierId)
    {
        var completed = await tenderService.AssignWinnerAsync(id, winningSupplierId);
        return completed ? NoContent() : NotFound();
    }
}