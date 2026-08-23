namespace TmsApi.Api.Controllers.V1;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TmsApi.Infrastructure.Identity;
using TmsApi.Infrastructure.Persistence;

[ApiController]
[Route("api/v1/users")]
[Authorize(Policy = "RequireAdminRole")]
public class UsersController : ControllerBase
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly TmsDbContext _dbContext;

    public UsersController(UserManager<ApplicationUser> userManager, TmsDbContext dbContext)
    {
        _userManager = userManager;
        _dbContext = dbContext;
    }

    [HttpGet]
    public async Task<IActionResult> GetUsers(
        [FromQuery] string? searchTerm,
        [FromQuery] string? role,
        [FromQuery] bool? isActive,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10)
    {
        var query = _userManager.Users.Where(u => !u.IsDeleted).AsQueryable();

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            var term = searchTerm.Trim().ToLower();
            query = query.Where(u => u.FullName.ToLower().Contains(term) || u.Email!.ToLower().Contains(term));
        }

        if (isActive.HasValue)
        {
            query = query.Where(u => u.IsActive == isActive.Value);
        }

        var totalCount = await query.CountAsync();
        var users = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var items = new List<object>();

        foreach (var user in users)
        {
            var roles = await _userManager.GetRolesAsync(user);

            if (!string.IsNullOrEmpty(role) && !roles.Contains(role, StringComparer.OrdinalIgnoreCase))
            {
                continue;
            }

            items.Add(new
            {
                id = user.Id,
                fullName = user.FullName,
                email = user.Email,
                roles = roles,
                isActive = user.IsActive,
                createdAt = DateTime.UtcNow // Map to your creation timestamp property if present
            });
        }

        return Ok(new
        {
            items,
            totalCount,
            pageNumber,
            pageSize
        });
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetUserById(int id)
    {
        var user = await _userManager.FindByIdAsync(id.ToString());
        if (user == null || user.IsDeleted) return NotFound();

        var roles = await _userManager.GetRolesAsync(user);

        return Ok(new
        {
            id = user.Id,
            fullName = user.FullName,
            email = user.Email,
            roles = roles,
            isActive = user.IsActive
        });
    }

    [HttpGet("logs")]
    public async Task<IActionResult> GetLoginActivityLogs([FromQuery] int? userId)
    {
        // Dummy/Placeholder array for activity logs if no persistent table exists yet
        return Ok(Array.Empty<object>());
    }
}