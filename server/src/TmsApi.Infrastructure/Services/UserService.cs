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
        // 1. Find user by email
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Email.ToLower() == dto.Email.ToLower() && !u.IsDeleted, cancellationToken);

        if (user == null || !user.IsActive)
        {
            throw new UnauthorizedAccessException("Invalid credentials or account inactive.");
        }

        // 2. Verify password hash using BCrypt
        bool isPasswordValid = BCrypt.Verify(dto.Password, user.PasswordHash);
        if (!isPasswordValid)
        {
            throw new UnauthorizedAccessException("Invalid credentials.");
        }

        // 3. Fetch user roles
        var roles = await _context.UserRoles
            .Where(ur => ur.UserId == user.Id)
            .Select(ur => ur.Role.Name)
            .ToListAsync(cancellationToken);

        // 4. Generate JWT token
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
        // 1. Check if user already exists
        var emailExists = await _context.Users
            .AnyAsync(u => u.Email.ToLower() == dto.Email.ToLower() && !u.IsDeleted, cancellationToken);

        if (emailExists)
        {
            throw new TmsApi.Application.Common.Exceptions.ConflictException($"A user with email '{dto.Email}' already exists.");
        }

        // 2. Validate role exists
        var role = await _context.Roles.FirstOrDefaultAsync(r => r.Name == "Bidder", cancellationToken);
        if (role == null)
        {
            throw new KeyNotFoundException($"Default role 'Bidder' was not found in the database.");
        }

        // 3. Hash password
        string hashedPassword = BCrypt.HashPassword(dto.Password);

        // 4. Instantiate entity using domain constructor
        var user = new User(dto.FullName, dto.Email, hashedPassword);
        _context.Users.Add(user);
        await _context.SaveChangesAsync(cancellationToken);

        // 5. Assign Role
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

    public async Task<IEnumerable<UserResponseDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var users = await _context.Users
            .Where(u => !u.IsDeleted)
            .ToListAsync(cancellationToken);

        var result = new List<UserResponseDto>();

        foreach (var user in users)
        {
            var roles = await _context.UserRoles
                .Where(ur => ur.UserId == user.Id)
                .Select(ur => ur.Role.Name)
                .ToListAsync(cancellationToken);

            result.Add(new UserResponseDto(user.Id, user.FullName, user.Email, user.IsActive, roles));
        }

        return result;
    }

    public async Task<bool> DeactivateUserAsync(int id, CancellationToken cancellationToken = default)
    {
        var user = await _context.Users.FindAsync(new object[] { id }, cancellationToken);
        if (user == null || user.IsDeleted) return false;

        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<bool> SoftDeleteUserAsync(int id, CancellationToken cancellationToken = default)
    {
        var user = await _context.Users.FindAsync(new object[] { id }, cancellationToken);
        if (user == null || user.IsDeleted) return false;

        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }
}