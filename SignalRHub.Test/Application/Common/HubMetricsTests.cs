namespace SignalRHub.Test.Application.Common;

public sealed class HubMetricsTests : IDisposable
{
    private readonly TestMeterFactory _meters = new();
    private readonly IConnectionRegistry _registry = Substitute.For<IConnectionRegistry>();
    private readonly HubMetrics _metrics;

    public HubMetricsTests()
    {
        _metrics = _meters.CreateHubMetrics(_registry);
    }

    [Fact]
    public void NotificationDelivered_records_sent_recipients_and_duration()
    {
        using MetricCollector<long> sent = _meters.Collect<long>("signalrhub.notifications.sent");
        using MetricCollector<long> recipients = _meters.Collect<long>("signalrhub.notifications.recipients");
        using MetricCollector<double> duration = _meters.Collect<double>("signalrhub.notifications.dispatch.duration");

        _metrics.NotificationDelivered(new ClientTarget(TestData.Client()), 3, TimeSpan.FromMilliseconds(250));

        CollectedMeasurement<long> sentMeasurement = sent.GetMeasurementSnapshot().Single();
        sentMeasurement.Value.ShouldBe(1);
        sentMeasurement.Tags["target"].ShouldBe("client");
        sentMeasurement.Tags["outcome"].ShouldBe("delivered");
        recipients.LastMeasurement!.Value.ShouldBe(3);
        duration.LastMeasurement!.Value.ShouldBe(0.25);
        duration.LastMeasurement.Tags["target"].ShouldBe("client");
    }

    [Fact]
    public void NotificationNotFound_records_not_found_outcome()
    {
        using MetricCollector<long> sent = _meters.Collect<long>("signalrhub.notifications.sent");

        _metrics.NotificationNotFound(new ConnectionTarget(TestData.Connection("c1")));

        sent.LastMeasurement!.Tags["target"].ShouldBe("connection");
        sent.LastMeasurement.Tags["outcome"].ShouldBe("not_found");
    }

    [Fact]
    public void Active_gauges_read_the_registry_statistics()
    {
        _registry.GetStatistics().Returns(new ConnectionRegistryStatistics(Clients: 2, Connections: 5));
        using MetricCollector<int> connections = _meters.Collect<int>("signalrhub.connections.active");
        using MetricCollector<int> clients = _meters.Collect<int>("signalrhub.clients.active");

        connections.RecordObservableInstruments();
        clients.RecordObservableInstruments();

        connections.LastMeasurement!.Value.ShouldBe(5);
        clients.LastMeasurement!.Value.ShouldBe(2);
    }

    [Fact]
    public void Connection_counters_record_opened_and_closed()
    {
        using MetricCollector<long> opened = _meters.Collect<long>("signalrhub.connections.opened");
        using MetricCollector<long> closed = _meters.Collect<long>("signalrhub.connections.closed");

        _metrics.ConnectionOpened();
        _metrics.ConnectionOpened();
        _metrics.ConnectionClosed();

        opened.GetMeasurementSnapshot().Sum(m => m.Value).ShouldBe(2);
        closed.GetMeasurementSnapshot().Sum(m => m.Value).ShouldBe(1);
    }

    public void Dispose() => _meters.Dispose();
}
