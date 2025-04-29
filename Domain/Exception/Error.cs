using System.Text.Json;

namespace Domain.Exception;

public class Error
{
    public string Title { get; set; }
    public int StatusCode { get; set; }
    public string Message { get; set; }
    public Guid TracdId { get; set; }
    
    public Error() { }

    public Error(string title, int statusCode, string message, Guid tracdId)
    {
        Title = title;
        StatusCode = statusCode;
        Message = message;
        TracdId = tracdId;
    }
    public override string ToString()
    {
        return JsonSerializer.Serialize(this);
    }
}