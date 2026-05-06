using System.Net;
using System.Net.Mail;

namespace BookManagement.Api.Helpers;

public interface IEmailSender
{
    Task SendEmailAsync(string email, string subject, string htmlMessage);
}

public class SmtpEmailSender : IEmailSender
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<SmtpEmailSender> _logger;

    public SmtpEmailSender(IConfiguration configuration, ILogger<SmtpEmailSender> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    public async Task SendEmailAsync(string email, string subject, string htmlMessage)
    {
        try
        {
            var host = _configuration["Smtp:Host"] ?? "smtp.gmail.com";
            var portString = _configuration["Smtp:Port"] ?? "587";
            var port = int.Parse(portString);
            var username = _configuration["Smtp:Username"];
            var password = _configuration["Smtp:Password"];

            if (string.IsNullOrWhiteSpace(username) || username == "YOUR_GMAIL@gmail.com")
            {
                _logger.LogWarning("SMTP Configuration is missing or using default placeholders. Email to {Email} will NOT be sent.", email);
                return;
            }

            using var client = new SmtpClient(host, port)
            {
                EnableSsl = true,
                Credentials = new NetworkCredential(username, password)
            };

            var mailMessage = new MailMessage
            {
                From = new MailAddress(username, "Book Management System"),
                Subject = subject,
                Body = htmlMessage,
                IsBodyHtml = true
            };

            mailMessage.To.Add(email);

            await client.SendMailAsync(mailMessage);
            _logger.LogInformation("Sent email to {Email}", email);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send email to {Email}", email);
            throw;
        }
    }
}
