using System.Text.Json;

namespace Domain.Exception;

public class Error
{
    public string Title { get; set; }
    public int Status { get; set; }
    public string? Message { get; set; }
    public Guid TracdId { get; set; }
    public Dictionary<string, string[]>? ErrorsValidate { get;set; }
    
    public Error() { }

    public Error(string title, int status, string? message, Dictionary<string, string[]>? errors, Guid tracdId)
    {
        Title = title;
        Status = status;
        Message = message;
        ErrorsValidate = errors;
        TracdId = tracdId;
    }
    public override string ToString()
    {
        return JsonSerializer.Serialize(this);
    }
}