using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using TmsCoreApi.Models;

namespace TmsCoreApi.Services;

public class UserService(ILogger<UserService> logger) : IUserService
{
    private readonly ConcurrentDictionary<string, User> _users = new();

    public Task<IEnumerable<User>> GetAllAsync()
    {
        logger.LogInformation("Retrieving all user accounts from storage.");
        return Task.FromResult(_users.Values.AsEnumerable());
    }

    public Task<User?> GetByIdAsync(string id)
    {
        var standardizedId = id.ToUpperInvariant();
        if (!_users.TryGetValue(standardizedId, out var user))
        {
            logger.LogWarning("User lookup failed. User identity with ID {UserId} was not found.", standardizedId);
            return Task.FromResult<User?>(null);
        }

        logger.LogInformation("Successfully retrieved user identity profile for ID {UserId}.", standardizedId);
        return Task.FromResult<User?>(user);
    }

    public Task<User> RegisterAsync(User user)
    {
        var standardizedId = user.Id.ToUpperInvariant();
        if (_users.ContainsKey(standardizedId))
        {
            logger.LogWarning("Registration conflict. User identity with ID {UserId} already exists.", standardizedId);
            throw new ArgumentException($"User with ID {standardizedId} is already registered.");
        }

        var newUser = new User
        {
            Id = standardizedId,
            Email = user.Email,
            Name = user.Name,
            Role = user.Role,
            IsActive = true
        };

        _users[standardizedId] = newUser;
        logger.LogInformation("Successfully registered new user: {UserName} with role {UserRole}.", newUser.Name, newUser.Role);
        return Task.FromResult(newUser);
    }

    public Task<bool> DeactivateAsync(string id)
    {
        var standardizedId = id.ToUpperInvariant();
        if (!_users.TryGetValue(standardizedId, out var user))
        {
            logger.LogWarning("Deactivation failed. Target User ID {UserId} not found.", standardizedId);
            return Task.FromResult(false);
        }

        user.IsActive = false;
        logger.LogInformation("Account status successfully updated to inactive for User ID {UserId}.", standardizedId);
        return Task.FromResult(true);
    }
}