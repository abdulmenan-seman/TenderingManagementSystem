namespace TmsApi.Api.Controllers.V1;

using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using TmsApi.Infrastructure.Identity;
using System.Security.Claims;
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
    private readonly UserManager<ApplicationUser> _userManager;

    public TendersController(ISender mediator, ICurrentUserService currentUser, ITmsDbContext context, UserManager<ApplicationUser> userManager)
    {
        _mediator = mediator;
        _currentUser = currentUser;
        _context = context;
        _userManager = userManager;
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
    [Authorize(Roles = "TenderOfficer,Admin,Bidder,Evaluator")]
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

    [HttpGet("evaluators")]
    [Authorize(Roles = "TenderOfficer,Admin")]
    public async Task<IActionResult> GetEvaluators(CancellationToken ct)
    {
        var evaluators = await _userManager.GetUsersInRoleAsync("Evaluator");
        return Ok(evaluators.Where(user => user.IsActive && !user.IsDeleted)
            .Select(user => new { id = user.Id, fullName = user.FullName, email = user.Email }));
    }

    [HttpGet("{id:int}/evaluation-criteria")]
    [Authorize(Roles = "TenderOfficer,Admin,Evaluator")]
    public async Task<IActionResult> GetTenderCriteria(int id, CancellationToken ct)
    {
        var criteria = await _context.EvaluationCriteria.AsNoTracking()
            .Where(criteria => criteria.TenderId == id)
            .Select(criteria => new { criteria.Id, criteria.CriteriaName, criteria.Description, criteria.WeightPercentage, criteria.MaxScore })
            .ToListAsync(ct);
        return Ok(criteria);
    }

    [HttpPost("{id:int}/evaluators/{evaluatorId:int}")]
    [Authorize(Roles = "TenderOfficer,Admin")]
    public async Task<IActionResult> AssignEvaluator(int id, int evaluatorId, CancellationToken ct)
    {
        var evaluator = await _userManager.FindByIdAsync(evaluatorId.ToString());
        if (evaluator is null || !await _userManager.IsInRoleAsync(evaluator, "Evaluator"))
            return BadRequest(new ProblemDetails { Detail = "The selected user is not an evaluator." });

        var tenderExists = await _context.Tenders.AnyAsync(tender => tender.Id == id && !tender.IsDeleted, ct);
        if (!tenderExists) return NotFound();

        var alreadyAssigned = await _context.TenderEvaluatorAssignments
            .AnyAsync(assignment => assignment.TenderId == id && assignment.EvaluatorId == evaluatorId, ct);
        if (!alreadyAssigned)
        {
            _context.TenderEvaluatorAssignments.Add(new TmsApi.Domain.Entities.TenderEvaluatorAssignment(id, evaluatorId));
            await _context.SaveChangesAsync(ct);
        }
        return Ok(new { message = "Evaluator assigned successfully." });
    }

    [HttpGet("assigned-to-me")]
    [Authorize(Roles = "Evaluator")]
    public async Task<IActionResult> GetAssignedToMe(CancellationToken ct)
    {
        if (!int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var evaluatorId))
            return Unauthorized();

        var tenderIds = _context.TenderEvaluatorAssignments
            .Where(assignment => assignment.EvaluatorId == evaluatorId)
            .Select(assignment => assignment.TenderId);

        var tenders = await _context.Tenders.AsNoTracking()
            .Where(tender => tenderIds.Contains(tender.Id) && !tender.IsDeleted)
            .OrderByDescending(tender => tender.Id)
            .Select(tender => new { tender.Id, tender.Title, Status = tender.Status.ToString() })
            .ToListAsync(ct);

        return Ok(tenders);
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