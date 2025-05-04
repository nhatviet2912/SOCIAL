namespace Application.Common.Interfaces.Service;

public interface IDateTimeService
{
    Task<DateTime> GetCurrentDateTimeAsync();
    
    DateTime Now { get; }
}