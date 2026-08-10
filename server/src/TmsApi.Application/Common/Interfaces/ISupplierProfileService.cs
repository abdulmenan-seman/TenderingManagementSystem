namespace TmsApi.Application.Common.Interfaces;

using TmsApi.Application.Auth.DTOs;

public interface ISupplierProfileService
{
    Task<SupplierProfileResponseDto> CreateProfileAsync(CreateSupplierProfileRequestDto dto, CancellationToken cancellationToken = default);
    Task<SupplierProfileResponseDto?> GetByUserIdAsync(int userId, CancellationToken cancellationToken = default);
    Task<SupplierProfileResponseDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<IEnumerable<SupplierProfileResponseDto>> GetAllAsync(CancellationToken cancellationToken = default);
}