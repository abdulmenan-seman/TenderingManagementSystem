namespace TmsApi.Application.Common.Interfaces;

/// <summary>
/// Provides access to the currently authenticated user's context from the active HTTP request.
/// </summary>
public interface ICurrentUserService
{
    /// <summary>Gets the unique ID of the current user, or null if unauthenticated.</summary>
    string? UserId { get; }

    /// <summary>Gets the username or email of the current user.</summary>
    string? UserName { get; }

    /// <summary>Checks if the current user belongs to a specific role (e.g., "Officer", "Supplier").</summary>
    bool IsInRole(string role);
}