using System;

namespace TmsApi.Domain.Entities;

public class Notification
{
    public int Id { get; private set; }
    public int UserId { get; private set; }
    public string Title { get; private set; } = default!;
    public string Message { get; private set; } = default!;
    public bool IsRead { get; private set; }
    public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;

    // Navigation property


    private Notification() { }

    public Notification(int userId, string title, string message)
    {
        UserId = userId;
        ArgumentException.ThrowIfNullOrWhiteSpace(title);
        ArgumentException.ThrowIfNullOrWhiteSpace(message);
        Title = title;
        Message = message;
        IsRead = false;
    }

    public void MarkAsRead() => IsRead = true;
}