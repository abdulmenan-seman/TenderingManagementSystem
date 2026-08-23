using Microsoft.AspNetCore.Identity;

namespace TmsApi.Infrastructure.Identity;

public class ApplicationUser : IdentityUser<int>
{
    public string FullName { get; set; } = default!;
    public bool IsActive { get; set; } = true;
    public bool IsDeleted { get; set; }
    
    // Refresh Token Support
    public string? RefreshToken { get; set; }
    public DateTime? RefreshTokenExpiryTime { get; set; }
}