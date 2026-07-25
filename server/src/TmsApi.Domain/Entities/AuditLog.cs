using System;

namespace TmsApi.Domain.Entities;

public class AuditLog
{
    public int Id { get; private set; }
    public int? UserId { get; private set; }
    public string Action { get; private set; } = default!; // e.g., "PUBLISHED_TENDER", "SUBMITTED_BID"
    public string EntityName { get; private set; } = default!; // e.g., "Tender"
    public string? EntityId { get; private set; }
    public string? IpAddress { get; private set; }
    public DateTime Timestamp { get; private set; } = DateTime.UtcNow;

    private AuditLog() { }

    public AuditLog(string action, string entityName, int? userId = null, string? entityId = null, string? ipAddress = null)
    {
        if (string.IsNullOrWhiteSpace(action)) throw new ArgumentException("Action must not be null or whitespace.", nameof(action));
        if (string.IsNullOrWhiteSpace(entityName)) throw new ArgumentException("EntityName must not be null or whitespace.", nameof(entityName));

        Action = action;
        EntityName = entityName;
        UserId = userId;
        EntityId = entityId;
        IpAddress = ipAddress;
    }
}