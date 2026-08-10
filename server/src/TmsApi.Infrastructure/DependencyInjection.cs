namespace TmsApi.Infrastructure;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Http;
using TmsApi.Application.Common.Interfaces;
using TmsApi.Infrastructure.Services;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services)
    {
        // Memory Cache registration
        services.AddMemoryCache();

        // HTTP Context for user claims parsing
        services.AddHttpContextAccessor();

        // Interface registrations
        services.AddScoped<ICurrentUserService, CurrentUserService>();
        services.AddScoped<IDateTimeService, DateTimeService>();
        services.AddScoped<INotificationService, NotificationService>();
        services.AddSingleton<ICacheService, MemoryCacheService>();

        return services;
    }
}