namespace TmsApi.Api.Controllers.V1;

using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TmsApi.Application.Auth.DTOs;
using TmsApi.Application.Users.QueriesAndCommands;

[ApiController]
[Route("api/v1/users")]
[Authorize]
public class UsersController : ControllerBase
{
    private readonly ISender _mediator;

    public UsersController(ISender mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var users = await _mediator.Send(new GetAllUsersQuery(), cancellationToken);
        return Ok(users);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id, CancellationToken cancellationToken)
    {
        var user = await _mediator.Send(new GetUserByIdQuery(id), cancellationToken);
        return user is not null ? Ok(user) : NotFound();
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> CreateUser([FromBody] CreateUserAdminDto dto, CancellationToken cancellationToken)
    {
        var user = await _mediator.Send(new CreateUserAdminCommand(dto), cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = user.Id }, user);
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> UpdateUser(int id, [FromBody] UpdateUserAdminDto dto, CancellationToken cancellationToken)
    {
        var user = await _mediator.Send(new UpdateUserAdminCommand(id, dto), cancellationToken);
        return user is not null ? Ok(user) : NotFound();
    }

    [HttpPatch("{id:int}/status")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> ToggleStatus(int id, [FromBody] ToggleStatusRequest request, CancellationToken cancellationToken)
    {
        var success = await _mediator.Send(new ToggleUserStatusCommand(id, request.IsActive), cancellationToken);
        return success ? NoContent() : NotFound();
    }

    [HttpPost("{id:int}/reset-password")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> ResetPassword(int id, [FromBody] ResetPasswordAdminRequest request, CancellationToken cancellationToken)
    {
        var success = await _mediator.Send(new ResetUserPasswordCommand(id, request.NewPassword), cancellationToken);
        return success ? NoContent() : NotFound();
    }

    [HttpPatch("{id:int}/deactivate")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Deactivate(int id, CancellationToken cancellationToken)
    {
        var success = await _mediator.Send(new DeactivateUserCommand(id), cancellationToken);
        return success ? NoContent() : NotFound();
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> SoftDelete(int id, CancellationToken cancellationToken)
    {
        var success = await _mediator.Send(new SoftDeleteUserCommand(id), cancellationToken);
        return success ? NoContent() : NotFound();
    }
}