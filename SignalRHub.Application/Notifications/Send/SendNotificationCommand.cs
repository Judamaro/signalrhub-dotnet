namespace SignalRHub.Application.Notifications.Send;

public sealed record SendNotificationCommand(NotificationTarget Target, string Type, JsonElement? Payload) : ICommand<SendNotificationResponse>;
