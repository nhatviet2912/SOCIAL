namespace Application.Common.Interfaces.Service;

public interface IEmailService
{
    Task<bool> SendEmailAsync(string email, string subject, string message);
}