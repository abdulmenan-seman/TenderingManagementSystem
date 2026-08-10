namespace TmsApi.Application.Auth.DTOs;

public record RegisterUserRequestDto(
    string FullName,
    string Email,
    string Password,
    int RoleId
);

public record CreateSupplierProfileRequestDto(
    int UserId,
    string CompanyName,
    string TaxIdNumber,
    string BusinessLicenseNumber,
    string Address,
    string PhoneNumber
);

public record UserResponseDto(
    int Id,
    string FullName,
    string Email,
    bool IsActive,
    List<string> Roles
);

public record SupplierProfileResponseDto(
    int Id,
    int UserId,
    string CompanyName,
    string TaxIdNumber,
    string BusinessLicenseNumber,
    string Address,
    string PhoneNumber
);