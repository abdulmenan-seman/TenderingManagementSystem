namespace TmsApi.Application.Users.QueriesAndCommands;

using MediatR;
using TmsApi.Application.Auth.DTOs;
using TmsApi.Application.Common.Interfaces;

public record GetUserByIdQuery(int Id) : IRequest<UserResponseDto?>;
public record GetAllUsersQuery() : IRequest<IEnumerable<UserResponseDto>>;
public record DeactivateUserCommand(int Id) : IRequest<bool>;
public record SoftDeleteUserCommand(int Id) : IRequest<bool>;

public class UserQueryAndCommandHandler :
    IRequestHandler<GetUserByIdQuery, UserResponseDto?>,
    IRequestHandler<GetAllUsersQuery, IEnumerable<UserResponseDto>>,
    IRequestHandler<DeactivateUserCommand, bool>,
    IRequestHandler<SoftDeleteUserCommand, bool>
{
    private readonly IUserService _userService;

    public UserQueryAndCommandHandler(IUserService userService) => _userService = userService;

    public Task<UserResponseDto?> Handle(GetUserByIdQuery request, CancellationToken cancellationToken)
        => _userService.GetByIdAsync(request.Id, cancellationToken);

    public Task<IEnumerable<UserResponseDto>> Handle(GetAllUsersQuery request, CancellationToken cancellationToken)
        => _userService.GetAllAsync(cancellationToken);

    public Task<bool> Handle(DeactivateUserCommand request, CancellationToken cancellationToken)
        => _userService.DeactivateUserAsync(request.Id, cancellationToken);

    public Task<bool> Handle(SoftDeleteUserCommand request, CancellationToken cancellationToken)
        => _userService.SoftDeleteUserAsync(request.Id, cancellationToken);
}