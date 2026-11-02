namespace SignalRHub.Application.Abstractions.Notifications;

/// <summary>
/// Output port that pushes a notification to a set of connections through the realtime transport.
/// </summary>
public interface INotificationDispatcher
{
    Task SendAsync(IReadOnlyCollection<ConnectionId> connectionIds, Notification notification, CancellationToken cancellationToken = default);
}
