using Application.Common.Interfaces.Service;

namespace Infrastructure.Service;

public class DateTimeService : IDateTimeService
{
    public Task<DateTime> GetCurrentDateTimeAsync() => Task.FromResult(DateTime.Now);
    public DateTime Now { get; }
}