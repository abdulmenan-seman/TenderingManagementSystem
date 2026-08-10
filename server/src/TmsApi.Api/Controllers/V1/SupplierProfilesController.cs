namespace TmsApi.Api.Controllers.V1;

using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TmsApi.Application.Auth.DTOs;
using TmsApi.Application.SupplierProfiles.QueriesAndCommands;

[ApiController]
[Route("api/v1/supplier-profiles")]
[Authorize]
public class SupplierProfilesController : ControllerBase
{
    private readonly ISender _mediator;

    public SupplierProfilesController(ISender mediator) => _mediator = mediator;

    [HttpPost]
    [EndpointSummary("Create supplier profile")]
    [ProducesResponseType(typeof(SupplierProfileResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateSupplierProfileRequestDto request, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new CreateSupplierProfileCommand(request), cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpGet("{id:int}")]
    [EndpointSummary("Get supplier profile by ID")]
    [ProducesResponseType(typeof(SupplierProfileResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id, CancellationToken cancellationToken)
    {
        var profile = await _mediator.Send(new GetSupplierProfileByIdQuery(id), cancellationToken);
        return profile is not null ? Ok(profile) : NotFound();
    }

    [HttpGet("user/{userId:int}")]
    [EndpointSummary("Get supplier profile by linked User ID")]
    [ProducesResponseType(typeof(SupplierProfileResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetByUserId(int userId, CancellationToken cancellationToken)
    {
        var profile = await _mediator.Send(new GetSupplierProfileByUserIdQuery(userId), cancellationToken);
        return profile is not null ? Ok(profile) : NotFound();
    }

    [HttpGet]
    [Authorize(Roles = "Admin,TenderOfficer")]
    [EndpointSummary("Get all supplier profiles")]
    [ProducesResponseType(typeof(IEnumerable<SupplierProfileResponseDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var profiles = await _mediator.Send(new GetAllSupplierProfilesQuery(), cancellationToken);
        return Ok(profiles);
    }
}