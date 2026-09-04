namespace TmsApi.Infrastructure;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using TmsApi.Application.Common.Interfaces;
using TmsApi.Infrastructure.Identity;
using TmsApi.Infrastructure.Persistence;
using TmsApi.Infrastructure.Services;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services)
    {
        // Memory Cache registration
        services.AddMemoryCache();

        // HTTP Context for user claims parsing
        services.AddHttpContextAccessor();

        // ASP.NET Core Identity Core Setup
        // - Uses AddIdentityCore to prevent duplicate cookie scheme registrations
        // - Automatically handles password hashing (PBKDF2 + salt)
        // - Provides UserManager<ApplicationUser> and RoleManager<ApplicationRole>
        services.AddIdentityCore<ApplicationUser>(options =>
        {
            // Password policy
            options.Password.RequireDigit           = true;
            options.Password.RequiredLength         = 8;
            options.Password.RequireUppercase       = true;
            options.Password.RequireLowercase       = true;
            options.Password.RequireNonAlphanumeric = true;

            // Lockout: 5 failed attempts → 15-minute lockout
            options.Lockout.MaxFailedAccessAttempts = 5;
            options.Lockout.DefaultLockoutTimeSpan  = TimeSpan.FromMinutes(15);
            options.Lockout.AllowedForNewUsers      = true;

            // Each email must be unique across all users
            options.User.RequireUniqueEmail = true;
        })
        .AddRoles<ApplicationRole>()
        .AddEntityFrameworkStores<TmsDbContext>()
        .AddDefaultTokenProviders();

        // Interface registrations
        services.AddScoped<ICurrentUserService, CurrentUserService>();
        services.AddScoped<IDateTimeService, DateTimeService>();
        services.AddScoped<INotificationService, NotificationService>();
        services.AddSingleton<ICacheService, MemoryCacheService>();
        services.AddScoped<IFileStorageService, LocalFileStorageService>();

        return services;
    }
}