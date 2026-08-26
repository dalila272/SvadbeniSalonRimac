using SvadbeniSalon.Model.Messages;

namespace SvadbeniSalon.Services;

public interface INotificationPublisher
{
    Task PublishAsync(NotificationMessage message, CancellationToken cancellationToken = default);
}
