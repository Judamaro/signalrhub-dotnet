namespace SignalRHub.Application.Notifications.Send;

public sealed record SendNotificationResponse(Guid NotificationId, int DeliveredConnections, DateTimeOffset SentAt);
