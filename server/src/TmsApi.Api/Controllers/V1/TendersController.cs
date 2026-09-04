namespace TmsApi.Api.Controllers.V1;

using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TmsApi.Application.Common.Interfaces;
using TmsApi.Application.Tenders.DTOs;
using TmsApi.Application.Tenders.QueriesAndCommands;

[ApiController]
[Route("api/v1/tenders")]
[Authorize]
public class TendersController : ControllerBase
{
    private readonly ISender _mediator;
    private readonly ICurrentUserService _currentUser;
    private readonly ITmsDbContext _context;

    public TendersController(ISender mediator, ICurrentUserService currentUser, ITmsDbContext context)
    {
        _mediator = mediator;
        _currentUser = currentUser;
        _context = context;
    }

    // ─── CREATE ──────────────────────────────────────────────────────────────

    /// <summary>Creates a new Draft tender. Officer ID is resolved from the JWT.</summary>
    [HttpPost]
    [Authorize(Roles = "TenderOfficer,Admin")]
    public async Task<IActionResult> Create([FromBody] CreateTenderRequestDto request, CancellationToken ct)
    {
        if (!int.TryParse(_currentUser.UserId, out var officerId))
            return Unauthorized("Could not resolve the current user's ID from the token.");

        var result = await _mediator.Send(new CreateTenderCommand(request, officerId), ct);
        return result.IsSuccess
            ? CreatedAtAction(nameof(GetById), new { id = result.Value?.Id }, result.Value)
            : BadRequest(result);
    }

    // ─── READ ────────────────────────────────────────────────────────────────

    /// <summary>Returns a paginated list of tenders, optionally filtered by status.</summary>
    [HttpGet]
    [Authorize(Roles = "TenderOfficer,Admin,Bidder")]
    public async Task<IActionResult> GetAll(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? status = null,
        CancellationToken ct = default)
    {
        // Bidders may only browse published tenders, regardless of the query string.
        if (User.IsInRole("Bidder"))
            status = "Published";

        var result = await _mediator.Send(new GetTendersQuery(pageNumber, pageSize, status), ct);
        return result.IsSuccess ? Ok(result.Value) : BadRequest(result);
    }

    /// <summary>Returns full detail for a single tender by its integer ID.</summary>
    [HttpGet("{id:int}")]
    [Authorize(Roles = "TenderOfficer,Admin,Bidder")]
    public async Task<IActionResult> GetById(int id, CancellationToken ct)
    {
        var result = await _mediator.Send(new GetTenderByIdQuery(id), ct);

        if (result.IsSuccess && User.IsInRole("Bidder") && result.Value?.Status != "Published")
            return NotFound();

        return result.IsSuccess ? Ok(result.Value) : NotFound(result);
    }

    // ─── UPDATE ──────────────────────────────────────────────────────────────

    /// <summary>Updates a Draft tender's core fields.</summary>
    [HttpPut("{id:int}")]
    [Authorize(Roles = "TenderOfficer,Admin")]
    public async Task<IActionResult> Update(int id, [FromBody] CreateTenderRequestDto request, CancellationToken ct)
    {
        var result = await _mediator.Send(new UpdateTenderCommand(id, request), ct);
        return result.IsSuccess ? Ok(result.Value) : BadRequest(result);
    }

    // ─── DELETE ──────────────────────────────────────────────────────────────

    /// <summary>Soft-deletes a Draft tender.</summary>
    [HttpDelete("{id:int}")]
    [Authorize(Roles = "TenderOfficer,Admin")]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        var result = await _mediator.Send(new DeleteTenderCommand(id), ct);
        return result.IsSuccess ? NoContent() : BadRequest(result);
    }

    // ─── WORKFLOW ACTIONS ────────────────────────────────────────────────────

    /// <summary>Publishes a Draft tender, making it visible to bidders.</summary>
    [HttpPost("{id:int}/publish")]
    [Authorize(Roles = "TenderOfficer,Admin")]
    public async Task<IActionResult> Publish(int id, CancellationToken ct)
    {
        var result = await _mediator.Send(new PublishTenderCommand(id), ct);
        return result.IsSuccess ? Ok() : BadRequest(result);
    }

    // ─── DOCUMENT UPLOAD ─────────────────────────────────────────────────────

    /// <summary>Uploads a document and attaches it to an existing tender.</summary>
    [HttpPost("{id:int}/documents")]
    [RequestSizeLimit(20 * 1024 * 1024)] // 20 MB max per file
    public async Task<IActionResult> UploadDocument(
        int id,
        [FromForm(Name = "file")] IFormFile? file,
        CancellationToken ct)
    {
        if (file is null || file.Length == 0)
            return BadRequest("No file was provided.");

        if (file.Length > 20 * 1024 * 1024)
            return BadRequest("The document cannot be larger than 20 MB.");

        await using var stream = file.OpenReadStream();
        using var memoryStream = new MemoryStream();
        await stream.CopyToAsync(memoryStream, ct);

        var command = new UploadTenderDocumentCommand(
            id,
            file.FileName,
            memoryStream.ToArray(),
            string.IsNullOrWhiteSpace(file.ContentType)
                ? "application/octet-stream"
                : file.ContentType);
        var result = await _mediator.Send(command, ct);

        return result.IsSuccess
            ? Ok(result.Value)
            : BadRequest(new ProblemDetails { Detail = result.Error ?? "Document upload failed." });
    }

    [HttpPost("{id:int}/start-evaluation")]
    [Authorize(Roles = "TenderOfficer,Admin")]
    public async Task<IActionResult> StartEvaluation(int id, CancellationToken ct)
    {
        var result = await _mediator.Send(new StartTenderEvaluationCommand(id), ct);
        return result.IsSuccess ? Ok() : BadRequest(result);
    }

    [HttpPost("{id:int}/close")]
    [Authorize(Roles = "TenderOfficer,Admin")]
    public async Task<IActionResult> Close(int id, CancellationToken ct)
    {
        var result = await _mediator.Send(new CloseTenderCommand(id), ct);
        return result.IsSuccess ? Ok() : BadRequest(result);
    }

    [HttpGet("documents/{documentId:int}/download")]
    [Authorize(Roles = "TenderOfficer,Admin,Bidder")]
    public async Task<IActionResult> DownloadDocument(int documentId, CancellationToken ct)
    {
        var document = await _context.TenderDocuments
            .Include(d => d.Tender)
            .AsNoTracking()
            .FirstOrDefaultAsync(d => d.Id == documentId && !d.Tender.IsDeleted, ct);

        if (document is null)
            return NotFound();

        if (User.IsInRole("Bidder") && document.Tender.Status != TmsApi.Domain.Entities.TenderStatus.Published)
            return NotFound();

        return File(document.Content, document.ContentType, document.FileName);
    }
}