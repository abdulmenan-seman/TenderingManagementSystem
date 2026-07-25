namespace TmsApi.Infrastructure.Services;

using TmsApi.Application.Common.Interfaces;
using TmsApi.Domain.Entities;

public class NotificationService : INotificationService
{
    private readonly ITmsDbContext _context;

    public NotificationService(ITmsDbContext context)
    {
        _context = context;
    }

    public async Task SendNotificationAsync(int recipientUserId, string title, string message, string type, CancellationToken cancellationToken = default)
    {
        var notification = new Notification(recipientUserId, message, type);
        _context.Notifications.Add(notification);
        await _context.SaveChangesAsync(cancellationToken);
    }
}