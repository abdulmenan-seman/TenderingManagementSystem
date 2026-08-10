namespace TmsApi.Api.Controllers.V1;

using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TmsApi.Application.Tenders.DTOs;
using TmsApi.Application.Tenders.QueriesAndCommands;

[ApiController]
[Route("api/v1/tenders/{tenderId:int}/documents")]
[Authorize(Roles = "TenderOfficer,Admin")]
public class TenderDocumentsController : ControllerBase
{
    private readonly ISender _mediator;

    public TenderDocumentsController(ISender mediator) => _mediator = mediator;

    [HttpPost]
    [EndpointSummary("Upload and attach a document to a tender")]
    [ProducesResponseType(typeof(TenderDocumentDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UploadDocument(
        int tenderId,
        [FromBody] UploadTenderDocumentRequestDto request,
        CancellationToken cancellationToken)
    {
        if (tenderId != request.TenderId)
        {
            return BadRequest(new ProblemDetails { Detail = "Route tenderId does not match request body TenderId." });
        }

        var result = await _mediator.Send(new UploadTenderDocumentCommand(request), cancellationToken);
        return CreatedAtAction(nameof(TendersController.GetById), "Tenders", new { id = tenderId }, result);
    }
}