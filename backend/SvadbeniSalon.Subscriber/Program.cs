using EasyNetQ;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SvadbeniSalon.Common.Services;
using SvadbeniSalon.Model.Messages;

EnvBootstrap.LoadRootEnvFile();

var builder = Host.CreateApplicationBuilder(args);

builder.Services.Configure<SmtpOptions>(builder.Configuration.GetSection(SmtpOptions.SectionName));
builder.Services.AddSingleton<IEmailService, SmtpEmailService>();

builder.Services.AddSingleton(_ =>
{
    var rabbitHost = builder.Configuration["RabbitMQ:Host"]
        ?? throw new InvalidOperationException("RabbitMQ:Host nije konfigurisan (.env / RABBITMQ_HOST).");
    var rabbitUser = builder.Configuration["RabbitMQ:Username"]
        ?? throw new InvalidOperationException("RabbitMQ:Username nije konfigurisan (.env).");
    var rabbitPass = builder.Configuration["RabbitMQ:Password"]
        ?? throw new InvalidOperationException("RabbitMQ:Password nije konfigurisan (.env).");
    var connectionString = $"host={rabbitHost};username={rabbitUser};password={rabbitPass}";
    return RabbitHutch.CreateBus(connectionString, x => x.EnableNewtonsoftJson());
});
builder.Services.AddHostedService<NotificationWorker>();

var host = builder.Build();
await host.RunAsync();

public class NotificationWorker : BackgroundService
{
    private static readonly TimeSpan[] ConnectionRetryDelays =
    [
        TimeSpan.FromSeconds(1),
        TimeSpan.FromSeconds(2),
        TimeSpan.FromSeconds(4),
        TimeSpan.FromSeconds(8),
    ];

    private static readonly TimeSpan[] SmtpRetryDelays =
    [
        TimeSpan.FromSeconds(1),
        TimeSpan.FromSeconds(2),
        TimeSpan.FromSeconds(4),
        TimeSpan.FromSeconds(8),
    ];

    private readonly IBus _bus;
    private readonly IEmailService _emailService;
    private readonly SmtpOptions _smtpOptions;
    private readonly ILogger<NotificationWorker> _logger;

    public NotificationWorker(
        IBus bus,
        IEmailService emailService,
        IOptions<SmtpOptions> smtpOptions,
        ILogger<NotificationWorker> logger)
    {
        _bus = bus;
        _emailService = emailService;
        _smtpOptions = smtpOptions.Value;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("SvadbeniSalon.Subscriber started.");

        var attempt = 0;
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await _bus.PubSub.SubscribeAsync<NotificationMessage>(
                    "email_notifications",
                    HandleMessageAsync,
                    cancellationToken: stoppingToken);

                attempt = 0;
                _logger.LogInformation("Subscriber connected. Waiting for messages...");
                await Task.Delay(Timeout.Infinite, stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                var delay = ConnectionRetryDelays[Math.Min(attempt, ConnectionRetryDelays.Length - 1)];
                attempt++;
                _logger.LogError(
                    ex,
                    "Subscriber nedostupan (pokušaj {Attempt}). Razlog: {Message}. Ponovni pokušaj za {Delay}s.",
                    attempt,
                    ex.Message,
                    delay.TotalSeconds);
                try
                {
                    await Task.Delay(delay, stoppingToken);
                }
                catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
                {
                    break;
                }
            }
        }

        _logger.LogInformation("SvadbeniSalon.Subscriber stopped.");
    }

    private async Task HandleMessageAsync(NotificationMessage message)
    {
        _logger.LogInformation(
            "Received notification Kind={Kind} Critical={Critical} UserId={UserId} Recipient={Recipient}",
            message.Kind,
            message.IsCritical,
            message.UserId,
            message.RecipientEmail ?? "(none)");

        if (!string.IsNullOrWhiteSpace(message.RecipientEmail))
        {
            await SendEmailWithRetryAsync(
                message.RecipientEmail,
                message.Title,
                message.Body,
                message.IsCritical);
        }
        else if (message.IsCritical)
        {
            _logger.LogError(
                "Critical notification without RecipientEmail. Kind={Kind} UserId={UserId}",
                message.Kind,
                message.UserId);
            throw new InvalidOperationException(
                "Critical notification missing RecipientEmail; message will be retried.");
        }

        if (!string.IsNullOrWhiteSpace(_smtpOptions.AdminEmail)
            && !string.IsNullOrWhiteSpace(message.AdminBody)
            && (message.Kind is "NewSvadba" or "SvadbaUpdated"))
        {
            try
            {
                await SendEmailWithRetryAsync(
                    _smtpOptions.AdminEmail,
                    $"[Admin] {message.Title}",
                    message.AdminBody,
                    critical: false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send admin email for svadba change");
            }
        }
    }

    private async Task SendEmailWithRetryAsync(
        string to,
        string subject,
        string body,
        bool critical)
    {
        Exception? lastError = null;

        for (var attempt = 0; attempt <= SmtpRetryDelays.Length; attempt++)
        {
            try
            {
                await _emailService.SendAsync(to, subject, body);
                if (attempt > 0)
                {
                    _logger.LogInformation(
                        "Email sent to {Recipient} after {Attempt} attempts: {Subject}",
                        to,
                        attempt + 1,
                        subject);
                }

                return;
            }
            catch (Exception ex) when (attempt < SmtpRetryDelays.Length)
            {
                lastError = ex;
                var delay = SmtpRetryDelays[attempt];
                _logger.LogWarning(
                    ex,
                    "SMTP send failed to {Recipient} (attempt {Attempt}/{Max}). Retry in {Delay}s. Critical={Critical}",
                    to,
                    attempt + 1,
                    SmtpRetryDelays.Length + 1,
                    delay.TotalSeconds,
                    critical);
                await Task.Delay(delay);
            }
            catch (Exception ex)
            {
                lastError = ex;
            }
        }

        _logger.LogError(
            lastError,
            "SMTP send permanently failed to {Recipient}: {Subject}. Critical={Critical}",
            to,
            subject,
            critical);

        if (critical)
        {
            // Rethrow so EasyNetQ can requeue / surface failure instead of losing the code.
            throw lastError
                  ?? new InvalidOperationException($"Failed to send critical email to {to}.");
        }
    }
}
