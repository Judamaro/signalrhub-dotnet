namespace SignalRHub.Domain.Notifications;

public static class NotificationErrors
{
    public static readonly Error TypeEmpty = Error.Validation("Notification.TypeEmpty", "The notification type is required.");

    public static readonly Error TypeTooLong = Error.Validation("Notification.TypeTooLong", $"The notification type cannot exceed {Notification.TypeMaxLength} characters.");

    public static Error UnsupportedTarget(NotificationTarget target)
        => Error.Failure("Notification.UnsupportedTarget", $"No delivery strategy is registered for '{target.GetType().Name}'.");
}
