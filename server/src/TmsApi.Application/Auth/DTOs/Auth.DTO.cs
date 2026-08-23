namespace TmsApi.Application.Auth.DTOs;

// Public Registration DTO for Bidders
public record RegisterBidderRequestDto(
    string Email,
    string Password,
    string FullName,
    string CompanyName,
    string ContactPerson,
    string BusinessLicenseNo,
    string TaxId,
    string Address,
    string PhoneNumber
);

// Admin Provisioning DTO for Staff (Admin, TenderOfficer, Evaluator)
public record CreateStaffUserRequestDto(
    string Email,
    string Password,
    string FullName,
    string Role // Must be: "Admin", "TenderOfficer", or "Evaluator"
);

public record LoginRequestDto(
    string Email,
    string Password
);

public record TokenResponseDto(
    string AccessToken,
    string RefreshToken,
    DateTime RefreshTokenExpiration,
    UserResponseDto User
);

public record RefreshTokenRequestDto(
    string AccessToken,
    string RefreshToken
);

public record UserResponseDto(
    int Id,
    string Email,
    string FullName,
    string Role,
    string? CompanyName
);


public record UpdateStaffUserRequestDto(
    string FullName,
    string Email,
    string Role,
    bool IsActive
);

public record ResetPasswordRequestDto(
    string NewPassword,
    bool MustChangePasswordOnLogin
);