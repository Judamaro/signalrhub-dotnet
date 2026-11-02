namespace SignalRHub.Infrastructure.Realtime;

/// <summary>
/// Wire contract received by SignalR clients in <see cref="INotificationClient.ReceiveNotification"/>.
/// </summary>
public sealed record NotificationMessage(Guid Id, string Type, JsonElement? Payload, DateTimeOffset CreatedAt)
{
    public static NotificationMessage From(Notification notification)
    {
        ArgumentNullException.ThrowIfNull(notification);

        return new NotificationMessage(notification.Id, notification.Type, notification.Payload, notification.CreatedAt);
    }
}
