namespace TmsApi.Application.Auth.DTOs;

public record CreateUserAdminDto(string FullName, string Email, string? Password, List<string> Roles);
public record UpdateUserAdminDto(string FullName, string Email, bool IsActive, List<string> Roles);
public record ToggleStatusRequest(bool IsActive);
public record ResetPasswordAdminRequest(string NewPassword);