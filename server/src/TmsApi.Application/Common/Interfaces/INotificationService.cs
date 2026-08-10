namespace TmsApi.Application.Common.Interfaces;

/// <summary>
/// Contract for sending cross-channel notifications (in-app, email, or background alerts).
/// </summary>
public interface INotificationService
{
    Task SendNotificationAsync(int recipientUserId, string title, string message, string type, CancellationToken cancellationToken = default);
}