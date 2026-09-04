namespace TmsApi.Api.Controllers.V1;

using System.Security.Claims;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;

using TmsApi.Application.Auth.DTOs;
using TmsApi.Domain.Entities;
using TmsApi.Infrastructure.Identity;
using TmsApi.Infrastructure.Persistence;

[ApiController]
[Route("api/v1/auth")]
[EnableRateLimiting("StrictPolicy")]
public class AuthController : ControllerBase
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<ApplicationRole> _roleManager;
    private readonly ITokenService _tokenService;
    private readonly TmsDbContext _dbContext;
    private readonly IConfiguration _config;

    public AuthController(
        UserManager<ApplicationUser> userManager,
        RoleManager<ApplicationRole> roleManager,
        ITokenService tokenService,
        TmsDbContext dbContext,
        IConfiguration config)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _tokenService = tokenService;
        _dbContext = dbContext;
        _config = config;
    }

    [HttpPost("register-bidder")]
    [AllowAnonymous]
    [EndpointSummary("Public self-registration for Bidders")]
    public async Task<IActionResult> RegisterBidder([FromBody] RegisterBidderRequestDto request)
    {
        var existingUser = await _userManager.FindByEmailAsync(request.Email);
        if (existingUser != null)
            return Conflict(new ProblemDetails { Detail = "Email address is already registered." });

        using var transaction = await _dbContext.Database.BeginTransactionAsync();
        try
        {
            var user = new ApplicationUser
            {
                UserName = request.Email,
                Email = request.Email,
                FullName = request.FullName,
                PhoneNumber = request.PhoneNumber,
                IsActive = true
            };

            var createResult = await _userManager.CreateAsync(user, request.Password);
            if (!createResult.Succeeded)
                return BadRequest(createResult.Errors);

            await _userManager.AddToRoleAsync(user, "Bidder");

            // Instantiate and persist BidderProfile linked to ApplicationUser.Id
            var bidderProfile = new BidderProfile(
                user.Id,
                request.CompanyName,
                request.ContactPerson,
                request.BusinessLicenseNo,
                request.TaxId,
                request.Address
            );

            _dbContext.BidderProfiles.Add(bidderProfile);
            await _dbContext.SaveChangesAsync();

            await transaction.CommitAsync();

            return CreatedAtAction(
                nameof(RegisterBidder), 
                new { id = user.Id }, 
                new UserResponseDto(user.Id, user.Email, user.FullName, "Bidder", request.CompanyName)
            );
        }
        catch (Exception)
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    [HttpPost("login")]
    [AllowAnonymous]
    [EndpointSummary("Authenticate user and generate JWT + Refresh Token")]
    public async Task<IActionResult> Login([FromBody] LoginRequestDto request)
    {
        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user == null || !user.IsActive || user.IsDeleted || !await _userManager.CheckPasswordAsync(user, request.Password))
            return Unauthorized(new ProblemDetails { Detail = "Invalid email or password." });

        var roles = await _userManager.GetRolesAsync(user);
        var primaryRole = roles.FirstOrDefault() ?? "Bidder";

        // Fetch company name if actor is a Bidder
        string? companyName = null;
        if (primaryRole == "Bidder")
        {
            var profile = await _dbContext.BidderProfiles.FirstOrDefaultAsync(b => b.UserId == user.Id);
            companyName = profile?.CompanyName;
        }

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Email, user.Email!),
            new(ClaimTypes.Name, user.FullName),
            new(ClaimTypes.Role, primaryRole)
        };

        var accessToken = _tokenService.GenerateAccessToken(claims);
        var refreshToken = _tokenService.GenerateRefreshToken();

        var expiryDays = double.Parse(_config["JwtSettings:RefreshTokenExpiryInDays"] ?? "7");
        user.RefreshToken = refreshToken;
        user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(expiryDays);
        await _userManager.UpdateAsync(user);

        return Ok(new TokenResponseDto(
            accessToken,
            refreshToken,
            user.RefreshTokenExpiryTime.Value,
            new UserResponseDto(user.Id, user.Email!, user.FullName, primaryRole, companyName)
        ));
    }

    [HttpPost("refresh")]
[AllowAnonymous]
[EndpointSummary("Rotate access and refresh tokens")]
public async Task<IActionResult> Refresh([FromBody] RefreshTokenRequestDto request)
{
    if (string.IsNullOrWhiteSpace(request.RefreshToken))
        return BadRequest(new ProblemDetails { Detail = "Refresh token is required." });

    // The access token is memory-only in the client and is unavailable after a hard refresh.
    var user = await _userManager.Users
        .FirstOrDefaultAsync(u => u.RefreshToken == request.RefreshToken);

    if (user == null || !user.IsActive || user.IsDeleted || 
        user.RefreshTokenExpiryTime <= DateTime.UtcNow)
    {
        return Unauthorized(new ProblemDetails { Detail = "Invalid or expired refresh token." });
    }

    var roles = await _userManager.GetRolesAsync(user);
    var primaryRole = roles.FirstOrDefault() ?? "Bidder";
    var claims = new List<Claim>
    {
        new(ClaimTypes.NameIdentifier, user.Id.ToString()),
        new(ClaimTypes.Email, user.Email!),
        new(ClaimTypes.Name, user.FullName),
        new(ClaimTypes.Role, primaryRole)
    };

    var newAccessToken = _tokenService.GenerateAccessToken(claims);
    var newRefreshToken = _tokenService.GenerateRefreshToken();

    var expiryDays = double.Parse(_config["JwtSettings:RefreshTokenExpiryInDays"] ?? "7");
    user.RefreshToken = newRefreshToken;
    user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(expiryDays);
    await _userManager.UpdateAsync(user);

    string? companyName = null;
    if (primaryRole == "Bidder")
    {
        var profile = await _dbContext.BidderProfiles.FirstOrDefaultAsync(b => b.UserId == user.Id);
        companyName = profile?.CompanyName;
    }

    return Ok(new TokenResponseDto(
        newAccessToken,
        newRefreshToken,
        user.RefreshTokenExpiryTime.Value,
        new UserResponseDto(user.Id, user.Email!, user.FullName, primaryRole, companyName)
    ));
}

    [HttpPost("revoke")]
    [Authorize]
    [EndpointSummary("Revoke refresh token session on logout")]
    public async Task<IActionResult> Revoke()
    {
        var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!int.TryParse(userIdString, out var userId)) return BadRequest();

        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user == null) return NotFound();

        user.RefreshToken = null;
        user.RefreshTokenExpiryTime = null;
        await _userManager.UpdateAsync(user);

        return Ok(new { message = "Session revoked successfully." });
    }

    [HttpPost("create-staff")]
    [Authorize(Policy = "RequireAdminRole")]
    [EndpointSummary("Provision Administrative Staff (Admin, TenderOfficer, Evaluator)")]
    public async Task<IActionResult> CreateStaff([FromBody] CreateStaffUserRequestDto request)
    {
        var validRoles = new[] { "Admin", "TenderOfficer", "Evaluator" };
        if (!validRoles.Contains(request.Role))
            return BadRequest(new ProblemDetails { Detail = "Invalid staff role specified." });

        var existingUser = await _userManager.FindByEmailAsync(request.Email);
        if (existingUser != null)
            return Conflict(new ProblemDetails { Detail = "User already exists with this email." });

        var user = new ApplicationUser
        {
            UserName = request.Email,
            Email = request.Email,
            FullName = request.FullName,
            IsActive = true
        };

        var result = await _userManager.CreateAsync(user, request.Password);
        if (!result.Succeeded)
            return BadRequest(result.Errors);

        await _userManager.AddToRoleAsync(user, request.Role);

        return Ok(new UserResponseDto(user.Id, user.Email, user.FullName, request.Role, null));
    }

    [HttpPut("staff/{id:int}")]
    [Authorize(Policy = "RequireAdminRole")]
    [EndpointSummary("Update Staff details and role assignment")]
    public async Task<IActionResult> UpdateStaff(int id, [FromBody] UpdateStaffUserRequestDto request)
    {
        var validRoles = new[] { "Admin", "TenderOfficer", "Evaluator" };
        if (!validRoles.Contains(request.Role))
            return BadRequest(new ProblemDetails { Detail = "Invalid staff role specified." });

        var user = await _userManager.FindByIdAsync(id.ToString());
        if (user == null || user.IsDeleted) return NotFound();

        user.FullName = request.FullName;
        user.Email = request.Email;
        user.UserName = request.Email;
        user.IsActive = request.IsActive;

        var updateResult = await _userManager.UpdateAsync(user);
        if (!updateResult.Succeeded) return BadRequest(updateResult.Errors);

        var currentRoles = await _userManager.GetRolesAsync(user);
        await _userManager.RemoveFromRolesAsync(user, currentRoles);
        await _userManager.AddToRoleAsync(user, request.Role);

        return Ok();
    }

    [HttpPatch("staff/{id:int}/status")]
    [Authorize(Policy = "RequireAdminRole")]
    [EndpointSummary("Toggle staff account activation status")]
    public async Task<IActionResult> ToggleStatus(int id, [FromBody] bool isActive)
    {
        var user = await _userManager.FindByIdAsync(id.ToString());
        if (user == null || user.IsDeleted) return NotFound();

        user.IsActive = isActive;
        await _userManager.UpdateAsync(user);
        return Ok();
    }

    [HttpPost("staff/{id:int}/reset-password")]
    [Authorize(Policy = "RequireAdminRole")]
    [EndpointSummary("Admin-forced password reset for staff member")]
    public async Task<IActionResult> ResetStaffPassword(int id, [FromBody] ResetPasswordRequestDto request)
    {
        var user = await _userManager.FindByIdAsync(id.ToString());
        if (user == null || user.IsDeleted) return NotFound();

        var token = await _userManager.GeneratePasswordResetTokenAsync(user);
        var result = await _userManager.ResetPasswordAsync(user, token, request.NewPassword);

        if (!result.Succeeded)
            return BadRequest(result.Errors);

        return Ok(new { message = "Password successfully reset." });
    }
}