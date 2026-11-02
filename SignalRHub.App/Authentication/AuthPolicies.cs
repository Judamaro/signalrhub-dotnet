namespace SignalRHub.App.Authentication;

public static class AuthPolicies
{
    /// <summary>Backends allowed to push notifications through the REST API.</summary>
    public const string NotificationPublisher = nameof(NotificationPublisher);
}
