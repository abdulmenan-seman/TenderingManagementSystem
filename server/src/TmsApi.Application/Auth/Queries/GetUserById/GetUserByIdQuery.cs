namespace TmsApi.Application.Auth.Queries.GetUserById;

using MediatR;
using Microsoft.EntityFrameworkCore;
using TmsApi.Application.Auth.DTOs;
using TmsApi.Application.Common.Interfaces;
using TmsApi.Application.Common.Models;

public record GetUserByIdQuery(int Id) : IRequest<Result<UserResponseDto>>;

public class GetUserByIdQueryHandler : IRequestHandler<GetUserByIdQuery, Result<UserResponseDto>>
{
    private readonly ITmsDbContext _context;

    public GetUserByIdQueryHandler(ITmsDbContext context)
    {
        _context = context;
    }

    public async Task<Result<UserResponseDto>> Handle(GetUserByIdQuery request, CancellationToken cancellationToken)
    {
        var user = await _context.Users
            .AsNoTracking()
            .Where(u => u.Id == request.Id)
            .FirstOrDefaultAsync(cancellationToken);

        if (user is null)
        {
            return Result<UserResponseDto>.Failure($"User with ID {request.Id} was not found.");
        }

        var roles = await _context.UserRoles
            .AsNoTracking()
            .Where(ur => ur.UserId == user.Id)
            .Select(ur => ur.Role.Name)
            .ToListAsync(cancellationToken);

        var dto = new UserResponseDto(user.Id, user.FullName, user.Email, user.IsActive, roles);
        return Result<UserResponseDto>.Success(dto);
    }
}