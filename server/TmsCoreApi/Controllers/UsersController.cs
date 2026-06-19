using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using TmsCoreApi.Models;
using TmsCoreApi.Services;

namespace TmsCoreApi.Controllers;

[ApiController]
[Route("api/users")]
public class UsersController(IUserService userService) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var users = await userService.GetAllAsync();
        return Ok(users);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(string id)
    {
        var user = await userService.GetByIdAsync(id);
        return user is not null ? Ok(user) : NotFound();
    }

    [HttpPost]
    public async Task<IActionResult> Register([FromBody] User user)
    {
        var createdUser = await userService.RegisterAsync(user);
        return CreatedAtAction(nameof(GetById), new { id = createdUser.Id }, createdUser);
    }

    [HttpPut("{id}/deactivate")]
    public async Task<IActionResult> Deactivate(string id)
    {
        var trackingResult = await userService.DeactivateAsync(id);
        return trackingResult ? NoContent() : NotFound();
    }
}