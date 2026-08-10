namespace TmsApi.Application.Common.Interfaces;

using TmsApi.Application.Auth.DTOs;

public interface IAuthService
{
    Task<AuthResponseDto> LoginAsync(LoginRequestDto dto, CancellationToken cancellationToken = default);
}