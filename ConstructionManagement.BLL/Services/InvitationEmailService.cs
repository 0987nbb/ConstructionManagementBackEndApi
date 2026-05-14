using Microsoft.Extensions.Configuration;
using System.Net;
using System.Net.Mail;

namespace ConstructionManagement.BLL.Services;

public class InvitationEmailService : IInvitationEmailService
{
    private readonly IConfiguration _configuration;

    public InvitationEmailService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public async Task SendWelcomeActivationEmailAsync(string toEmail, string fullName, string setupUrl, DateTime expiresAtUtc)
    {
        var host = _configuration["Smtp:Host"];
        var fromEmail = _configuration["Smtp:FromEmail"];
        var fromName = _configuration["Smtp:FromName"] ?? "Construction ERP";
        var username = _configuration["Smtp:Username"];
        var password = _configuration["Smtp:Password"];
        var port = int.TryParse(_configuration["Smtp:Port"], out var parsedPort) ? parsedPort : 587;
        var enableSsl = !string.Equals(_configuration["Smtp:EnableSsl"], "false", StringComparison.OrdinalIgnoreCase);

        if (string.IsNullOrWhiteSpace(host) || string.IsNullOrWhiteSpace(fromEmail))
        {
            throw new InvalidOperationException("SMTP settings are incomplete. Configure Smtp:Host and Smtp:FromEmail.");
        }

        using var message = new MailMessage();
        message.From = new MailAddress(fromEmail, fromName);
        message.To.Add(new MailAddress(toEmail));
        message.Subject = "Welcome to Construction ERP - Activate Your Account";
        message.IsBodyHtml = true;

        var expiryText = expiresAtUtc.ToString("dddd, dd MMM yyyy 'at' HH:mm 'UTC'");
        message.Body = $"""
            <p>Hello {WebUtility.HtmlEncode(fullName)},</p>
            <p>Welcome to Construction ERP. Your account has been created by an administrator.</p>
            <p>Please activate your account and set your password by clicking the secure link below:</p>
            <p><a href="{WebUtility.HtmlEncode(setupUrl)}">Activate My Account</a></p>
            <p>This secure link expires on {WebUtility.HtmlEncode(expiryText)}.</p>
            <p>If you did not expect this invitation, please ignore this email and contact your administrator.</p>
            <p>Regards,<br/>Construction ERP Team</p>
            """;

        using var client = new SmtpClient(host, port) { EnableSsl = enableSsl };
        if (!string.IsNullOrWhiteSpace(username))
        {
            client.Credentials = new NetworkCredential(username, password ?? string.Empty);
        }

        await client.SendMailAsync(message);
    }
}
