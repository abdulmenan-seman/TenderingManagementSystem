namespace TmsApi.Infrastructure.Services;

using TmsApi.Application.Common.Interfaces;

public class DateTimeService : IDateTimeService
{
    public DateTime UtcNow => DateTime.UtcNow;
}