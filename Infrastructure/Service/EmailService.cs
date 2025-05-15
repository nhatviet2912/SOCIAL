using Application.Common.Interfaces.Service;
using Domain.Entities;
using Domain.Entities.Settings;
using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;

namespace Infrastructure.Service;

public class EmailService : IEmailService
{
    private readonly EmailSettings _emailSettings;
    public EmailService(EmailSettings emailSettings)
    {
        _emailSettings = emailSettings;
    }
    
    public async Task<bool> SendEmailAsync(string email, string subject, string message)
    {
        var emailMessage = new MimeMessage();
        emailMessage.From.Add(new MailboxAddress(_emailSettings.FromName, _emailSettings.FromAddress));
        emailMessage.To.Add(new MailboxAddress("", email));
        emailMessage.Subject = subject;
        emailMessage.Body = new TextPart("html") { Text = message };
            
        using (var client = new SmtpClient())
        {
            try
            {
                await client.ConnectAsync(_emailSettings.Host, _emailSettings.Port, SecureSocketOptions.StartTls);
                await client.AuthenticateAsync(_emailSettings.UserName, _emailSettings.Password);
                await client.SendAsync(emailMessage);

                return await Task.FromResult(true);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
            finally
            {
                await client.DisconnectAsync(true);
                client.Dispose();
            }
        }
    }
}