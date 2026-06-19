using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using TmsCoreApi.Models;
using TmsCoreApi.Services;

namespace TmsCoreApi.Controllers;

[ApiController]
[Route("api/bids")]
public class BidsController(IBidService bidService) : ControllerBase
{
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(string id)
    {
        var bid = await bidService.GetByIdAsync(id);
        return bid is not null ? Ok(bid) : NotFound();
    }

    [HttpGet("tender/{tenderId}")]
    public async Task<IActionResult> GetByTender(string tenderId)
    {
        var bids = await bidService.GetBidsByTenderIdAsync(tenderId);
        return Ok(bids);
    }

    [HttpPost]
    public async Task<IActionResult> Submit([FromBody] Bid bid)
    {
        var createdBid = await bidService.SubmitBidAsync(bid);
        return CreatedAtAction(nameof(GetById), new { id = createdBid.Id }, createdBid);
    }

    [HttpPatch("{id}/status")]
    public async Task<IActionResult> ChangeStatus(string id, [FromBody] BidStatus status)
    {
        var processed = await bidService.UpdateBidStatusAsync(id, status);
        return processed ? NoContent() : NotFound();
    }
}