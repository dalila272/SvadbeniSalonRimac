using EasyNetQ;
using SvadbeniSalon.Model.Messages;
using SvadbeniSalon.Services;

namespace SvadbeniSalon.WebAPI.Services;

public class RabbitMqNotificationPublisher : INotificationPublisher
{
    private readonly IBus _bus;
    private readonly ILogger<RabbitMqNotificationPublisher> _logger;

    public RabbitMqNotificationPublisher(IBus bus, ILogger<RabbitMqNotificationPublisher> logger)
    {
        _bus = bus;
        _logger = logger;
    }

    public async Task PublishAsync(NotificationMessage message, CancellationToken cancellationToken = default)
    {
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
            _logger.LogWarning(ex, "Failed to publish notification to RabbitMQ");
        }
    }
}
