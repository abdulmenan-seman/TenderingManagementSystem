namespace TmsApi.Application.Common.Interfaces;

using TmsApi.Application.Auth.DTOs;

public interface IUserService
{
    Task<AuthResponseDto> LoginAsync(LoginRequestDto dto, CancellationToken cancellationToken = default);
    Task<UserResponseDto> RegisterUserAsync(RegisterUserRequestDto dto, CancellationToken cancellationToken = default);
    Task<UserResponseDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<IEnumerable<UserResponseDto>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<bool> DeactivateUserAsync(int id, CancellationToken cancellationToken = default);
    Task<bool> SoftDeleteUserAsync(int id, CancellationToken cancellationToken = default);
}