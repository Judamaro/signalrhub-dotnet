namespace SignalRHub.Infrastructure.Realtime;

/// <summary>
/// Strongly typed contract of the methods the server invokes on connected clients.
/// </summary>
public interface INotificationClient
{
    Task ReceiveNotification(NotificationMessage notification);
}
