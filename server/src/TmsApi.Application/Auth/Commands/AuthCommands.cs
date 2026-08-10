namespace TmsApi.Application.Auth.Commands;

using MediatR;
using TmsApi.Application.Auth.DTOs;
using TmsApi.Application.Common.Interfaces;

public record RegisterUserCommand(RegisterUserRequestDto Dto) : IRequest<UserResponseDto>;

public class RegisterUserCommandHandler : IRequestHandler<RegisterUserCommand, UserResponseDto>
{
    private readonly IUserService _userService;

    public RegisterUserCommandHandler(IUserService userService) => _userService = userService;

    public Task<UserResponseDto> Handle(RegisterUserCommand request, CancellationToken cancellationToken)
        => _userService.RegisterUserAsync(request.Dto, cancellationToken);
}

public record LoginCommand(LoginRequestDto Dto) : IRequest<AuthResponseDto>;

public class LoginCommandHandler : IRequestHandler<LoginCommand, AuthResponseDto>
{
    private readonly IUserService _userService;

    public LoginCommandHandler(IUserService userService) => _userService = userService;

    public Task<AuthResponseDto> Handle(LoginCommand request, CancellationToken cancellationToken)
        => _userService.LoginAsync(request.Dto, cancellationToken);
}