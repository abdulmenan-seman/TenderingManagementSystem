namespace TmsApi.Application.Auth.Commands.RegisterUser;

using MediatR;
using Microsoft.EntityFrameworkCore;
using TmsApi.Application.Auth.DTOs;
using TmsApi.Application.Common.Interfaces;
using TmsApi.Application.Common.Models;
using TmsApi.Domain.Entities;

public record RegisterUserCommand(RegisterUserRequestDto UserDto) : IRequest<Result<int>>;

public class RegisterUserCommandHandler : IRequestHandler<RegisterUserCommand, Result<int>>
{
    private readonly ITmsDbContext _context;

    public RegisterUserCommandHandler(ITmsDbContext context)
    {
        _context = context;
    }

    public async Task<Result<int>> Handle(RegisterUserCommand request, CancellationToken cancellationToken)
    {
        var dto = request.UserDto;

        var emailExists = await _context.Users.AnyAsync(u => u.Email == dto.Email, cancellationToken);
        if (emailExists)
        {
            return Result<int>.Failure($"Email '{dto.Email}' is already registered.");
        }

        var role = await _context.Roles.FindAsync(new object[] { dto.RoleId }, cancellationToken);
        if (role is null)
        {
            return Result<int>.Failure($"Role with ID {dto.RoleId} does not exist.");
        }

        try
        {
            // Simple password assignment placeholder — password hashing service will be injected in Infrastructure
            var user = new User(dto.FullName, dto.Email, dto.Password);
            _context.Users.Add(user);
            await _context.SaveChangesAsync(cancellationToken);

            var userRole = new UserRole(user.Id, role.Id);
            _context.UserRoles.Add(userRole);
            await _context.SaveChangesAsync(cancellationToken);

            return Result<int>.Success(user.Id);
        }
        catch (ArgumentException ex)
        {
            return Result<int>.Failure(ex.Message);
        }
    }
}