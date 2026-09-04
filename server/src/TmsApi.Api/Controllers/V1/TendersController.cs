namespace TmsApi.Api.Controllers.V1;

using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TmsApi.Application.Common.Interfaces;
using TmsApi.Application.Tenders.DTOs;
using TmsApi.Application.Tenders.QueriesAndCommands;

[ApiController]
[Route("api/v1/tenders")]
[Authorize(Roles = "TenderOfficer,Admin")]
public class TendersController : ControllerBase
{
    private readonly ISender _mediator;
    private readonly ICurrentUserService _currentUser;

    public TendersController(ISender mediator, ICurrentUserService currentUser)
    {
        _mediator = mediator;
        _currentUser = currentUser;
    }

    // ─── CREATE ──────────────────────────────────────────────────────────────

    /// <summary>Creates a new Draft tender. Officer ID is resolved from the JWT.</summary>
    [HttpPost]
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
    public async Task<IActionResult> GetAll(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? status = null,
        CancellationToken ct = default)
    {
        var result = await _mediator.Send(new GetTendersQuery(pageNumber, pageSize, status), ct);
        return result.IsSuccess ? Ok(result.Value) : BadRequest(result);
    }

    /// <summary>Returns full detail for a single tender by its integer ID.</summary>
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id, CancellationToken ct)
    {
        var result = await _mediator.Send(new GetTenderByIdQuery(id), ct);
        return result.IsSuccess ? Ok(result.Value) : NotFound(result);
    }

    // ─── UPDATE ──────────────────────────────────────────────────────────────

    /// <summary>Updates a Draft tender's core fields.</summary>
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] CreateTenderRequestDto request, CancellationToken ct)
    {
        var result = await _mediator.Send(new UpdateTenderCommand(id, request), ct);
        return result.IsSuccess ? Ok(result.Value) : BadRequest(result);
    }

    // ─── DELETE ──────────────────────────────────────────────────────────────

    /// <summary>Soft-deletes a Draft tender.</summary>
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        var result = await _mediator.Send(new DeleteTenderCommand(id), ct);
        return result.IsSuccess ? NoContent() : BadRequest(result);
    }

    // ─── WORKFLOW ACTIONS ────────────────────────────────────────────────────

    /// <summary>Publishes a Draft tender, making it visible to bidders.</summary>
    [HttpPost("{id:int}/publish")]
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
        IFormFile file,
        [FromServices] IFileStorageService fileStorage,
        CancellationToken ct)
    {
        if (file is null || file.Length == 0)
            return BadRequest("No file was provided.");

        using var stream = file.OpenReadStream();
        var savedFilePath = await fileStorage.SaveFileAsync(stream, file.FileName, "tenders", ct);

        var command = new UploadTenderDocumentCommand(id, file.FileName, savedFilePath);
        var result = await _mediator.Send(command, ct);

        return result.IsSuccess ? Ok(result.Value) : BadRequest(result);
    }
}