namespace TmsApi.Domain.Entities;

public class UserRole
{
    public int UserId { get; private set; }
    public int RoleId { get; private set; }

    // Navigation properties
    public User User { get; private set; } = default!;
    public Role Role { get; private set; } = default!;

    private UserRole() { }

    public UserRole(int userId, int roleId)
    {
        UserId = userId;
        RoleId = roleId;
    }
}