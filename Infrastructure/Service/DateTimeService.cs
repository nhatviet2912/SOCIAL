using Application.Common.Interfaces.Service;

namespace Infrastructure.Service;

public class DateTimeService : IDateTimeService
{
    public DateTime GetCurrentDateTimeAsync() => DateTime.Now;
}