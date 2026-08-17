namespace TmsApi.Infrastructure.Services;

using BCrypt.Net;
using Microsoft.EntityFrameworkCore;
using TmsApi.Application.Auth.DTOs;
using TmsApi.Application.Common.Interfaces;
using TmsApi.Domain.Entities;
using TmsApi.Infrastructure.Persistence;

public class UserService : IUserService
{
    private readonly TmsDbContext _context;
    private readonly IJwtTokenGenerator _jwtTokenGenerator;

    public UserService(TmsDbContext context, IJwtTokenGenerator jwtTokenGenerator)
    {
        _context = context;
        _jwtTokenGenerator = jwtTokenGenerator;
    }

    public async Task<AuthResponseDto> LoginAsync(LoginRequestDto dto, CancellationToken cancellationToken = default)
    {
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Email.ToLower() == dto.Email.Trim().ToLower() && !u.IsDeleted, cancellationToken);

        if (user == null || !user.IsActive)
        {
            throw new UnauthorizedAccessException("Invalid credentials or account inactive.");
        }

        bool isPasswordValid = BCrypt.Verify(dto.Password, user.PasswordHash);
        if (!isPasswordValid)
        {
            throw new UnauthorizedAccessException("Invalid credentials.");
        }

        var roles = await _context.UserRoles
            .Where(ur => ur.UserId == user.Id)
            .Select(ur => ur.Role.Name)
            .ToListAsync(cancellationToken);

        string token = _jwtTokenGenerator.GenerateToken(user, roles);

        return new AuthResponseDto(
            user.Id,
            user.FullName,
            user.Email,
            token,
            roles
        );
    }

    public async Task<UserResponseDto> RegisterUserAsync(RegisterUserRequestDto dto, CancellationToken cancellationToken = default)
    {
        var emailExists = await _context.Users
            .AnyAsync(u => u.Email.ToLower() == dto.Email.Trim().ToLower() && !u.IsDeleted, cancellationToken);

        if (emailExists)
        {
            throw new TmsApi.Application.Common.Exceptions.ConflictException($"A user with email '{dto.Email}' already exists.");
        }

        var role = await _context.Roles.FirstOrDefaultAsync(r => r.Name == "Bidder", cancellationToken);
        if (role == null)
        {
            throw new KeyNotFoundException($"Default role 'Bidder' was not found in the database.");
        }

        string hashedPassword = BCrypt.HashPassword(dto.Password);
        var user = new User(dto.FullName, dto.Email.Trim(), hashedPassword);

        _context.Users.Add(user);
        await _context.SaveChangesAsync(cancellationToken);

        var userRole = new UserRole(user.Id, role.Id);
        _context.UserRoles.Add(userRole);
        await _context.SaveChangesAsync(cancellationToken);

        return new UserResponseDto(
            user.Id,
            user.FullName,
            user.Email,
            user.IsActive,
            new List<string> { role.Name }
        );
    }

    public async Task<IEnumerable<UserResponseDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var users = await _context.Users
            .Where(u => !u.IsDeleted)
            .Select(u => new UserResponseDto(
                u.Id,
                u.FullName,
                u.Email,
                u.IsActive,
                _context.UserRoles
                    .Where(ur => ur.UserId == u.Id)
                    .Select(ur => ur.Role.Name)
                    .ToList()
            ))
            .ToListAsync(cancellationToken);

        return users;
    }

    public async Task<UserResponseDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var user = await _context.Users
            .Where(u => u.Id == id && !u.IsDeleted)
            .FirstOrDefaultAsync(cancellationToken);

        if (user == null) return null;

        var roles = await _context.UserRoles
            .Where(ur => ur.UserId == id)
            .Select(ur => ur.Role.Name)
            .ToListAsync(cancellationToken);

        return new UserResponseDto(user.Id, user.FullName, user.Email, user.IsActive, roles);
    }

    public async Task<UserResponseDto> CreateUserByAdminAsync(CreateUserAdminDto dto, CancellationToken cancellationToken = default)
    {
        var emailExists = await _context.Users
            .AnyAsync(u => u.Email.ToLower() == dto.Email.Trim().ToLower() && !u.IsDeleted, cancellationToken);

        if (emailExists)
        {
            throw new TmsApi.Application.Common.Exceptions.ConflictException($"Email '{dto.Email}' is already registered.");
        }

        string tempPassword = string.IsNullOrWhiteSpace(dto.Password) ? "P@ssword123!" : dto.Password;
        string hashedPassword = BCrypt.HashPassword(tempPassword);

        var user = new User(dto.FullName, dto.Email.Trim(), hashedPassword);
        _context.Users.Add(user);
        await _context.SaveChangesAsync(cancellationToken);

        if (dto.Roles != null && dto.Roles.Any())
        {
            var rolesToAssign = await _context.Roles
                .Where(r => dto.Roles.Contains(r.Name))
                .ToListAsync(cancellationToken);

            foreach (var r in rolesToAssign)
            {
                _context.UserRoles.Add(new UserRole(user.Id, r.Id));
            }
            await _context.SaveChangesAsync(cancellationToken);
        }

        return new UserResponseDto(user.Id, user.FullName, user.Email, user.IsActive, dto.Roles ?? new List<string>());
    }

    public async Task<UserResponseDto?> UpdateUserAsync(int id, UpdateUserAdminDto dto, CancellationToken cancellationToken = default)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == id && !u.IsDeleted, cancellationToken);
        if (user == null) return null;

        // Use Domain Method
        user.UpdateProfile(dto.FullName, dto.Email.Trim(), dto.IsActive);

        var existingUserRoles = await _context.UserRoles.Where(ur => ur.UserId == id).ToListAsync(cancellationToken);
        _context.UserRoles.RemoveRange(existingUserRoles);

        var rolesToAssign = await _context.Roles
            .Where(r => dto.Roles.Contains(r.Name))
            .ToListAsync(cancellationToken);

        foreach (var r in rolesToAssign)
        {
            _context.UserRoles.Add(new UserRole(user.Id, r.Id));
        }

        await _context.SaveChangesAsync(cancellationToken);

        return new UserResponseDto(user.Id, user.FullName, user.Email, user.IsActive, dto.Roles);
    }

    public async Task<bool> ToggleUserStatusAsync(int id, bool isActive, CancellationToken cancellationToken = default)
    {
        var user = await _context.Users.FindAsync(new object[] { id }, cancellationToken);
        if (user == null || user.IsDeleted) return false;

        user.SetActiveStatus(isActive);
        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<bool> ResetPasswordAsync(int id, string newPassword, CancellationToken cancellationToken = default)
    {
        var user = await _context.Users.FindAsync(new object[] { id }, cancellationToken);
        if (user == null || user.IsDeleted) return false;

        user.SetPasswordHash(BCrypt.HashPassword(newPassword));
        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<bool> DeactivateUserAsync(int id, CancellationToken cancellationToken = default)
    {
        var user = await _context.Users.FindAsync(new object[] { id }, cancellationToken);
        if (user == null || user.IsDeleted) return false;

        user.Deactivate();
        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<bool> SoftDeleteUserAsync(int id, CancellationToken cancellationToken = default)
    {
        var user = await _context.Users.FindAsync(new object[] { id }, cancellationToken);
        if (user == null || user.IsDeleted) return false;

        user.SoftDelete();
        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }
}