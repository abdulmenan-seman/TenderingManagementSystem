namespace TmsApi.Application.Auth.DTOs;

public record LoginRequestDto(
    string Email,
    string Password
);

public record AuthResponseDto(
    int UserId,
    string FullName,
    string Email,
    string Token,
    List<string> Roles
);