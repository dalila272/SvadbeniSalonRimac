using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MimeKit;

namespace SvadbeniSalon.Common.Services;

public class SmtpEmailService : IEmailService
{
    private readonly SmtpOptions _options;
    private readonly ILogger<SmtpEmailService> _logger;

    public SmtpEmailService(IOptions<SmtpOptions> options, ILogger<SmtpEmailService> logger)
    {
        _options = options.Value;
        _logger = logger;
    }

    public async Task SendAsync(
        string to,
        string subject,
        string body,
        CancellationToken cancellationToken = default)
    {
        if (!_options.IsConfigured)
        {
            _logger.LogWarning("SMTP is not configured; skipping email to {Recipient}", to);
            return;
        }

        if (string.IsNullOrWhiteSpace(to))
        {
            _logger.LogWarning("Recipient email is empty; skipping send.");
            return;
        }

        using var message = new MimeMessage();
        message.From.Add(MailboxAddress.Parse(_options.From!));
        message.To.Add(MailboxAddress.Parse(to.Trim()));
        message.Subject = subject;
        message.Body = new TextPart("plain") { Text = body };

        using var client = new SmtpClient();
        var secureSocket = _options.UseSsl ? SecureSocketOptions.StartTls : SecureSocketOptions.Auto;

        await client.ConnectAsync(_options.Host!, _options.Port, secureSocket, cancellationToken);

        if (!string.IsNullOrWhiteSpace(_options.Username))
        {
            await client.AuthenticateAsync(_options.Username, _options.Password, cancellationToken);
        }

        await client.SendAsync(message, cancellationToken);
        await client.DisconnectAsync(true, cancellationToken);

        _logger.LogInformation("Email sent to {Recipient}: {Subject}", to, subject);
    }
}
