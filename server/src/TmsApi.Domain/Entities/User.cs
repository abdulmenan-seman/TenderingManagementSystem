namespace TmsApi.Domain.Entities;

public class User
{
    public int Id { get; private set; }
    public string FullName { get; private set; } = default!;
    public string Email { get; private set; } = default!;
    public string PasswordHash { get; private set; } = default!;
    public bool IsActive { get; private set; } = true;
    public bool IsDeleted { get; private set; }

    private User() { }

    public User(string fullName, string email, string passwordHash)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(fullName);
        ArgumentException.ThrowIfNullOrWhiteSpace(email);
        ArgumentException.ThrowIfNullOrWhiteSpace(passwordHash);

        FullName = fullName;
        Email = email;
        PasswordHash = passwordHash;
    }
}