namespace TmsApi.Application.Users.QueriesAndCommands;

using MediatR;
using TmsApi.Application.Auth.DTOs;
using TmsApi.Application.Common.Interfaces;

public record GetUserByIdQuery(int Id) : IRequest<UserResponseDto?>;
public record GetAllUsersQuery() : IRequest<IEnumerable<UserResponseDto>>;
public record CreateUserAdminCommand(CreateUserAdminDto Dto) : IRequest<UserResponseDto>;
public record UpdateUserAdminCommand(int Id, UpdateUserAdminDto Dto) : IRequest<UserResponseDto?>;
public record ToggleUserStatusCommand(int Id, bool IsActive) : IRequest<bool>;
public record ResetUserPasswordCommand(int Id, string NewPassword) : IRequest<bool>;
public record DeactivateUserCommand(int Id) : IRequest<bool>;
public record SoftDeleteUserCommand(int Id) : IRequest<bool>;

public class UserQueryAndCommandHandler :
    IRequestHandler<GetUserByIdQuery, UserResponseDto?>,
    IRequestHandler<GetAllUsersQuery, IEnumerable<UserResponseDto>>,
    IRequestHandler<CreateUserAdminCommand, UserResponseDto>,
    IRequestHandler<UpdateUserAdminCommand, UserResponseDto?>,
    IRequestHandler<ToggleUserStatusCommand, bool>,
    IRequestHandler<ResetUserPasswordCommand, bool>,
    IRequestHandler<DeactivateUserCommand, bool>,
    IRequestHandler<SoftDeleteUserCommand, bool>
{
    private readonly IUserService _userService;

    public UserQueryAndCommandHandler(IUserService userService) => _userService = userService;

    public Task<UserResponseDto?> Handle(GetUserByIdQuery request, CancellationToken cancellationToken)
        => _userService.GetByIdAsync(request.Id, cancellationToken);

    public Task<IEnumerable<UserResponseDto>> Handle(GetAllUsersQuery request, CancellationToken cancellationToken)
        => _userService.GetAllAsync(cancellationToken);

    public Task<UserResponseDto> Handle(CreateUserAdminCommand request, CancellationToken cancellationToken)
        => _userService.CreateUserByAdminAsync(request.Dto, cancellationToken);

    public Task<UserResponseDto?> Handle(UpdateUserAdminCommand request, CancellationToken cancellationToken)
        => _userService.UpdateUserAsync(request.Id, request.Dto, cancellationToken);

    public Task<bool> Handle(ToggleUserStatusCommand request, CancellationToken cancellationToken)
        => _userService.ToggleUserStatusAsync(request.Id, request.IsActive, cancellationToken);

    public Task<bool> Handle(ResetUserPasswordCommand request, CancellationToken cancellationToken)
        => _userService.ResetPasswordAsync(request.Id, request.NewPassword, cancellationToken);

    public Task<bool> Handle(DeactivateUserCommand request, CancellationToken cancellationToken)
        => _userService.DeactivateUserAsync(request.Id, cancellationToken);

    public Task<bool> Handle(SoftDeleteUserCommand request, CancellationToken cancellationToken)
        => _userService.SoftDeleteUserAsync(request.Id, cancellationToken);
}