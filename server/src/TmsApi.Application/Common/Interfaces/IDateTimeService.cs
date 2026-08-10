namespace TmsApi.Application.Common.Interfaces;

/// <summary>
/// Abstraction for system clock operations to enable deterministic time testing.
/// </summary>
public interface IDateTimeService
{
    DateTime UtcNow { get; }
}