namespace ConstructionManagement.BLL.Services;

public interface IInvitationEmailService
{
    Task SendWelcomeActivationEmailAsync(string toEmail, string fullName, string setupUrl, DateTime expiresAtUtc);
    Task SendPasswordResetEmailAsync(string toEmail, string fullName, string resetUrl, DateTime expiresAtUtc);
}
