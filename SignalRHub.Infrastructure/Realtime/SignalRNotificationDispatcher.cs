namespace SignalRHub.Infrastructure.Realtime;

/// <summary>
/// Adapter from the <see cref="INotificationDispatcher"/> port to SignalR.
/// </summary>
internal sealed class SignalRNotificationDispatcher(IHubContext<NotificationHub, INotificationClient> hubContext) : INotificationDispatcher
{
    public Task SendAsync(IReadOnlyCollection<ConnectionId> connectionIds, Notification notification, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(connectionIds);

        if (connectionIds.Count == 0)
            return Task.CompletedTask;

        string[] ids = connectionIds.Select(c => c.Value).ToArray();

        return hubContext.Clients.Clients(ids).ReceiveNotification(NotificationMessage.From(notification));
    }
}
