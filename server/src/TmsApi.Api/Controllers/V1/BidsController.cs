namespace TmsApi.Api.Controllers.V1;

using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using TmsApi.Application.Bids.Commands.AddBidDocument;
using TmsApi.Application.Bids.Commands.SubmitBid;
using TmsApi.Application.Bids.DTOs;
using TmsApi.Application.Bids.Queries.GetBidsByTenderId;
using TmsApi.Application.Common.Interfaces;
using TmsApi.Infrastructure.Persistence;

[ApiController]
[Route("api/v1/bids")]
[Authorize]
public class BidsController : ControllerBase
{
    private readonly ISender _mediator;
    private readonly TmsDbContext _context;

    public BidsController(ISender mediator, TmsDbContext context)
    {
        _mediator = mediator;
        _context = context;
    }

    [HttpPost]
    [Authorize(Roles = "Bidder")]
    public async Task<IActionResult> SubmitBid([FromBody] SubmitBidRequestDto dto, CancellationToken cancellationToken)
    {
        if (!int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var bidderId))
            return Unauthorized();

        var result = await _mediator.Send(new SubmitBidCommand(bidderId, dto), cancellationToken);

        if (!result.IsSuccess)
            return BadRequest(new ProblemDetails { Detail = result.Error });

        return Ok(new { BidId = result.Value });
    }

    [HttpPost("{bidId:int}/documents")]
    [Authorize(Roles = "Bidder")]
    [RequestSizeLimit(20 * 1024 * 1024)]
    public async Task<IActionResult> AddDocument(int bidId, [FromForm] string documentType, [FromForm(Name = "file")] IFormFile? file, CancellationToken cancellationToken)
    {
        if (!int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var bidderId))
            return Unauthorized();
        if (file is null || file.Length == 0)
            return BadRequest(new ProblemDetails { Detail = "No document file was provided." });
        if (file.Length > 20 * 1024 * 1024)
            return BadRequest(new ProblemDetails { Detail = "The document cannot be larger than 20 MB." });

        var ownsBid = await _context.Bids.AnyAsync(b => b.Id == bidId && b.SupplierId == bidderId && !b.IsDeleted, cancellationToken);
        if (!ownsBid)
            return NotFound();

        await using var stream = file.OpenReadStream();
        using var memoryStream = new MemoryStream();
        await stream.CopyToAsync(memoryStream, cancellationToken);
        var dto = new AddBidDocumentRequestDto(bidId, documentType, file.FileName, null!, memoryStream.ToArray(), file.ContentType);
        var result = await _mediator.Send(new AddBidDocumentCommand(dto), cancellationToken);

        if (!result.IsSuccess)
            return BadRequest(new ProblemDetails { Detail = result.Error });

        return Ok(new { DocumentId = result.Value });
    }

    [HttpGet("mine")]
    [Authorize(Roles = "Bidder")]
    public async Task<IActionResult> GetMine(CancellationToken cancellationToken)
    {
        if (!int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var bidderId))
            return Unauthorized();

        var bids = await _context.Bids
            .AsNoTracking()
            .Include(b => b.Documents)
            .Include(b => b.Tender)
            .Where(b => b.SupplierId == bidderId && !b.IsDeleted)
            .OrderByDescending(b => b.SubmissionDate)
            .ToListAsync(cancellationToken);

        return Ok(bids.Select(b => new BidResponseDto(
            b.Id, b.TenderId, b.SupplierId, b.FinancialProposalAmount, b.SubmissionDate,
            b.Status.ToString(), b.Documents.Select(d => new BidDocumentDto(
                d.Id, d.DocumentType, d.FileName, $"/api/v1/bids/documents/{d.Id}/download", d.UploadedAt)).ToList())));
    }

    [HttpGet("documents/{documentId:int}/download")]
    [Authorize(Roles = "Bidder,TenderOfficer,Admin,Evaluator")]
    public async Task<IActionResult> DownloadDocument(int documentId, CancellationToken cancellationToken)
    {
        var document = await _context.BidDocuments.Include(d => d.Bid)
            .FirstOrDefaultAsync(d => d.Id == documentId && !d.Bid.IsDeleted, cancellationToken);
        if (document is null) return NotFound();

        if (User.IsInRole("Bidder") && (!int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var bidderId) || document.Bid.SupplierId != bidderId))
            return NotFound();

        return File(document.Content, document.ContentType, document.FileName);
    }

    [HttpDelete("{bidId:int}")]
    [Authorize(Roles = "Bidder")]
    public async Task<IActionResult> Withdraw(int bidId, CancellationToken cancellationToken)
    {
        if (!int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var bidderId))
            return Unauthorized();

        var bid = await _context.Bids.FirstOrDefaultAsync(
            b => b.Id == bidId && b.SupplierId == bidderId && !b.IsDeleted, cancellationToken);
        if (bid is null) return NotFound();

        try
        {
            bid.Withdraw();
            await _context.SaveChangesAsync(cancellationToken);
            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new ProblemDetails { Detail = ex.Message });
        }
    }

    [HttpGet("tender/{tenderId:int}")]
    [Authorize(Roles = "TenderOfficer,Evaluator,Admin")]
    public async Task<IActionResult> GetByTenderId(int tenderId, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetBidsByTenderIdQuery(tenderId), cancellationToken);

        if (!result.IsSuccess)
            return NotFound(new ProblemDetails { Detail = result.Error });

        return Ok(result.Value);
    }
}