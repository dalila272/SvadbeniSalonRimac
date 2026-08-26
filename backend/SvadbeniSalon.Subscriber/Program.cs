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
    private static readonly TimeSpan[] RetryDelays =
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
                var delay = RetryDelays[Math.Min(attempt, RetryDelays.Length - 1)];
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
            "Received notification Kind={Kind} UserId={UserId} Recipient={Recipient}",
            message.Kind,
            message.UserId,
            message.RecipientEmail ?? "(none)");

        if (!string.IsNullOrWhiteSpace(message.RecipientEmail))
        {
            try
            {
                await _emailService.SendAsync(
                    message.RecipientEmail,
                    message.Title,
                    message.Body);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send email to {Recipient}", message.RecipientEmail);
            }
        }

        if (!string.IsNullOrWhiteSpace(_smtpOptions.AdminEmail)
            && !string.IsNullOrWhiteSpace(message.AdminBody)
            && (message.Kind is "NewSvadba" or "SvadbaUpdated"))
        {
            try
            {
                await _emailService.SendAsync(
                    _smtpOptions.AdminEmail,
                    $"[Admin] {message.Title}",
                    message.AdminBody);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send admin email for svadba change");
            }
        }
    }
}
