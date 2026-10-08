using EasyNetQ;
using Microsoft.AspNetCore.SignalR;
using SvadbeniSalon.Model.Exceptions;
using SvadbeniSalon.Model.Messages;
using SvadbeniSalon.Services;
using SvadbeniSalon.WebAPI.Hubs;

namespace SvadbeniSalon.WebAPI.Services;

public class RabbitMqNotificationPublisher : INotificationPublisher
{
    private static readonly TimeSpan[] PublishRetryDelays =
    [
        TimeSpan.FromSeconds(1),
        TimeSpan.FromSeconds(2),
        TimeSpan.FromSeconds(4),
        TimeSpan.FromSeconds(8),
    ];

    private readonly IBus _bus;
    private readonly INotifikacijaService _notifikacijaService;
    private readonly IHubContext<NotificationHub> _hub;
    private readonly ILogger<RabbitMqNotificationPublisher> _logger;

    public RabbitMqNotificationPublisher(
        IBus bus,
        INotifikacijaService notifikacijaService,
        IHubContext<NotificationHub> hub,
        ILogger<RabbitMqNotificationPublisher> logger)
    {
        _bus = bus;
        _notifikacijaService = notifikacijaService;
        _hub = hub;
        _logger = logger;
    }

    public async Task PublishAsync(NotificationMessage message, CancellationToken cancellationToken = default)
    {
        if (message.CreatedAt == default)
        {
            message.CreatedAt = DateTime.UtcNow;
        }

        try
        {
            if (message.UserId > 0)
            {
                var saved = await _notifikacijaService.CreateFromMessageAsync(message);
                await _hub.Clients
                    .User(message.UserId.ToString())
                    .SendAsync("notificationReceived", saved, cancellationToken);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to persist/push in-app notification for user {UserId}", message.UserId);
            if (message.IsCritical)
            {
                throw;
            }
        }

        if (message.IsCritical)
        {
            await PublishToRabbitWithRetryAsync(message, cancellationToken);
            return;
        }

        try
        {
            await _bus.PubSub.PublishAsync(message, cancellationToken);
            _logger.LogInformation(
                "Notification published for user {UserId}: {Title}",
                message.UserId,
                message.Title);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to publish notification to RabbitMQ (non-critical)");
        }
    }

    private async Task PublishToRabbitWithRetryAsync(
        NotificationMessage message,
        CancellationToken cancellationToken)
    {
        Exception? lastError = null;

        for (var attempt = 0; attempt <= PublishRetryDelays.Length; attempt++)
        {
            try
            {
                await _bus.PubSub.PublishAsync(message, cancellationToken);
                _logger.LogInformation(
                    "Critical notification published for user {UserId}: {Title} (attempt {Attempt})",
                    message.UserId,
                    message.Title,
                    attempt + 1);
                return;
            }
            catch (Exception ex) when (attempt < PublishRetryDelays.Length)
            {
                lastError = ex;
                var delay = PublishRetryDelays[attempt];
                _logger.LogWarning(
                    ex,
                    "Critical RabbitMQ publish failed (attempt {Attempt}/{Max}). Retry in {Delay}s.",
                    attempt + 1,
                    PublishRetryDelays.Length + 1,
                    delay.TotalSeconds);
                await Task.Delay(delay, cancellationToken);
            }
            catch (Exception ex)
            {
                lastError = ex;
            }
        }

        _logger.LogError(
            lastError,
            "Critical notification could not be published to RabbitMQ after retries. UserId={UserId} Kind={Kind}",
            message.UserId,
            message.Kind);

        throw new ClientException(
            "Privremeno nije moguće poslati email. Pokušajte ponovo za par minuta.");
    }
}
