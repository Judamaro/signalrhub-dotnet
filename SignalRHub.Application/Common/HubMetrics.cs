namespace SignalRHub.Application.Common;

/// <summary>
/// Business metrics of the hub, emitted through the <see cref="MeterName"/> meter with
/// <see cref="System.Diagnostics.Metrics"/>. The exporter (Prometheus) is configured in the App layer.
/// </summary>
public sealed class HubMetrics
{
    /// <summary>Name of the meter that groups the hub metrics.</summary>
    public const string MeterName = "SignalRHub";

    private const string TargetTag = "target";
    private const string OutcomeTag = "outcome";

    private readonly Counter<long> _connectionsOpened;
    private readonly Counter<long> _connectionsClosed;
    private readonly Counter<long> _notificationsSent;
    private readonly Counter<long> _notificationRecipients;
    private readonly Histogram<double> _dispatchDuration;

    /// <summary>Initializes a new instance of the <see cref="HubMetrics"/> class.</summary>
    /// <param name="meterFactory">Factory that creates the meter, owned by the dependency injection container.</param>
    /// <param name="registry">Registry read by the active clients and connections gauges.</param>
    public HubMetrics(IMeterFactory meterFactory, IConnectionRegistry registry)
    {
        ArgumentNullException.ThrowIfNull(meterFactory);
        ArgumentNullException.ThrowIfNull(registry);

        Meter meter = meterFactory.Create(MeterName);

        _connectionsOpened = meter.CreateCounter<long>("signalrhub.connections.opened", "{connection}", "Connections registered in the hub.");
        _connectionsClosed = meter.CreateCounter<long>("signalrhub.connections.closed", "{connection}", "Connections removed from the hub.");
        meter.CreateObservableUpDownCounter("signalrhub.connections.active", () => registry.GetStatistics().Connections, "{connection}", "Connections currently registered in the hub.");
        meter.CreateObservableUpDownCounter("signalrhub.clients.active", () => registry.GetStatistics().Clients, "{client}", "Clients with at least one connection registered in the hub.");
        _notificationsSent = meter.CreateCounter<long>("signalrhub.notifications.sent", "{notification}", "Notification requests, by target and outcome.");
        _notificationRecipients = meter.CreateCounter<long>("signalrhub.notifications.recipients", "{connection}", "Connections notifications were delivered to.");
        _dispatchDuration = meter.CreateHistogram<double>("signalrhub.notifications.dispatch.duration", "s", "Time SignalR took to dispatch a notification to its connections.");
    }

    /// <summary>Records a connection registered in the hub.</summary>
    public void ConnectionOpened() => _connectionsOpened.Add(1);

    /// <summary>Records a connection removed from the hub.</summary>
    public void ConnectionClosed() => _connectionsClosed.Add(1);

    /// <summary>Records a notification delivered to its connections.</summary>
    /// <param name="target">Target of the notification.</param>
    /// <param name="recipients">Number of connections the notification was delivered to.</param>
    /// <param name="duration">Time SignalR took to dispatch the notification.</param>
    public void NotificationDelivered(NotificationTarget target, int recipients, TimeSpan duration)
    {
        string targetName = GetTargetName(target);

        _notificationsSent.Add(1, new(TargetTag, targetName), new(OutcomeTag, "delivered"));
        _notificationRecipients.Add(recipients, new KeyValuePair<string, object?>(TargetTag, targetName));
        _dispatchDuration.Record(duration.TotalSeconds, new KeyValuePair<string, object?>(TargetTag, targetName));
    }

    /// <summary>Records a notification rejected because its client has no connections or its connection does not exist.</summary>
    /// <param name="target">Target of the notification.</param>
    public void NotificationNotFound(NotificationTarget target) =>
        _notificationsSent.Add(1, new(TargetTag, GetTargetName(target)), new(OutcomeTag, "not_found"));

    private static string GetTargetName(NotificationTarget target) => target switch
    {
        ClientTarget => "client",
        ConnectionTarget => "connection",
        _ => "other",
    };
}
