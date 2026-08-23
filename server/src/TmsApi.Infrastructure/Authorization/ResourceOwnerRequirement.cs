namespace TmsApi.Infrastructure.Authorization;

using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

public record ResourceOwnerRequirement : IAuthorizationRequirement;

public class ResourceOwnerAuthorizationHandler : AuthorizationHandler<ResourceOwnerRequirement, int>
{
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context, 
        ResourceOwnerRequirement requirement, 
        int resourceOwnerUserId)
    {
        var userIdClaim = context.User.FindFirstValue(ClaimTypes.NameIdentifier);

        // Allow Admins and Procurement Officers global access
        if (context.User.IsInRole("Admin") || context.User.IsInRole("TenderOfficer"))
        {
            context.Succeed(requirement);
            return Task.CompletedTask;
        }

        // Verify resource owner ID matches the current user's ID
        if (int.TryParse(userIdClaim, out var currentUserId) && currentUserId == resourceOwnerUserId)
        {
            context.Succeed(requirement);
        }

        return Task.CompletedTask;
    }
}