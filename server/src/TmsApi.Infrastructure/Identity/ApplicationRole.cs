using Microsoft.AspNetCore.Identity;

namespace TmsApi.Infrastructure.Identity;

public class ApplicationRole : IdentityRole<int>
{
    public ApplicationRole() { }
    public ApplicationRole(string roleName) : base(roleName) { }
}
