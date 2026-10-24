
using System.Diagnostics;
using System.Net;
using System.Net.Mail;
using Microsoft.AspNetCore.Identity.UI.Services;

namespace EddaStore.Services;

public class EmailSender(
    IConfiguration configuration,
    ILogger<EmailSender> logger) : IEmailSender
{
    public async Task SendEmailAsync(
        string email,
        string subject,
        string htmlMessage)
    {
        var host = configuration["EmailSettings:Host"];
        var username = configuration["EmailSettings:Username"];
        var appPassword = configuration["EmailSettings:AppPassword"];
        var fromEmail = configuration["EmailSettings:Email"] ?? username;
        var displayName = configuration["EmailSettings:DisplayName"];

        if (string.IsNullOrWhiteSpace(host) ||
            string.IsNullOrWhiteSpace(username) ||
            string.IsNullOrWhiteSpace(appPassword) ||
            string.IsNullOrWhiteSpace(fromEmail))
        {
            logger.LogError(
                "SMTP configuration is incomplete. Host={HasHost}, Username={HasUsername}, Password={HasPassword}, From={HasFrom}",
                !string.IsNullOrWhiteSpace(host),
                !string.IsNullOrWhiteSpace(username),
                !string.IsNullOrWhiteSpace(appPassword),
                !string.IsNullOrWhiteSpace(fromEmail));

            throw new InvalidOperationException(
                "SMTP configuration is incomplete. Check User Secrets.");
        }

        var port = configuration.GetValue<int?>("EmailSettings:Port") ?? 587;
        var enableSsl = configuration.GetValue<bool?>("EmailSettings:EnableSsl") ?? true;

        logger.LogInformation(
            "Starting SMTP send. Recipient={Recipient}, Host={Host}, Port={Port}, FromMatchesUsername={Matches}",
            MaskEmail(email),
            host,
            port,
            string.Equals(fromEmail, username, StringComparison.OrdinalIgnoreCase));

        var timer = Stopwatch.StartNew();

        try
        {
            using var client = new SmtpClient(host, port)
            {
                EnableSsl = enableSsl,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential(username, appPassword),
                Timeout = 30000
            };

            var fromAddress = string.IsNullOrWhiteSpace(displayName)
                ? new MailAddress(fromEmail)
                : new MailAddress(fromEmail, displayName);

            using var message = new MailMessage(fromAddress, new MailAddress(email))
            {
                Subject = subject,
                Body = htmlMessage,
                IsBodyHtml = true
            };

            await client.SendMailAsync(message);

            logger.LogInformation(
                "SMTP server accepted message. Recipient={Recipient}, ElapsedMs={ElapsedMs}",
                MaskEmail(email),
                timer.ElapsedMilliseconds);
        }
        catch (SmtpException ex)
        {
            logger.LogError(
                ex,
                "SMTP error. StatusCode={StatusCode}, Recipient={Recipient}",
                ex.StatusCode,
                MaskEmail(email));

            throw;
        }
        catch (Exception ex)
        {
            logger.LogError(
                ex,
                "Unexpected email sending error. Recipient={Recipient}",
                MaskEmail(email));

            throw;
        }
    }

    private static string MaskEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            return "<empty>";

        var atIndex = email.IndexOf('@');

        return atIndex <= 1
            ? "***"
            : string.Concat(email[..1], "***", email[atIndex..]);
    }
}
