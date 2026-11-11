namespace SignalRHub.Test.Application.Notifications;

public sealed class SendNotificationCommandHandlerTests : IDisposable
{
    private static readonly string[] ExpectedConnections = ["c1", "c2"];

    private readonly IConnectionRegistry _registry = Substitute.For<IConnectionRegistry>();
    private readonly INotificationDispatcher _dispatcher = Substitute.For<INotificationDispatcher>();
    private readonly TestMeterFactory _meters = new();
    private readonly SendNotificationCommandHandler _handler;

    public SendNotificationCommandHandlerTests()
    {
        DeliveryStrategyResolver resolver = new(
        [
            new ClientDeliveryStrategy(_registry),
            new ConnectionDeliveryStrategy(_registry),
        ]);

        _handler = new SendNotificationCommandHandler(resolver, _dispatcher, new FakeTimeProvider(TestData.Now), _meters.CreateHubMetrics(_registry));
    }

    [Fact]
    public async Task Sends_to_every_connection_of_the_client()
    {
        ClientId client = TestData.Client();
        _registry.GetByClientAsync(client, Arg.Any<CancellationToken>())
            .Returns([TestData.ClientConnection("c1"), TestData.ClientConnection("c2")]);
        using MetricCollector<long> sent = _meters.Collect<long>("signalrhub.notifications.sent");
        using MetricCollector<long> recipients = _meters.Collect<long>("signalrhub.notifications.recipients");

        Result<SendNotificationResponse> result = await _handler.Handle(new SendNotificationCommand(new ClientTarget(client), "order.updated", null), CancellationToken.None);

        result.IsSuccess.ShouldBeTrue();
        result.Value.DeliveredConnections.ShouldBe(2);
        result.Value.SentAt.ShouldBe(TestData.Now);
        await _dispatcher.Received(1).SendAsync(
            Arg.Is<IReadOnlyCollection<ConnectionId>>(ids => ids.Select(i => i.Value).Order().SequenceEqual(ExpectedConnections)),
            Arg.Is<Notification>(n => n.Type == "order.updated"),
            Arg.Any<CancellationToken>());
        sent.LastMeasurement!.Tags["outcome"].ShouldBe("delivered");
        sent.LastMeasurement.Tags["target"].ShouldBe("client");
        recipients.LastMeasurement!.Value.ShouldBe(2);
    }

    [Fact]
    public async Task Fails_with_not_found_when_client_has_no_connections()
    {
        ClientId client = TestData.Client("ghost");
        _registry.GetByClientAsync(client, Arg.Any<CancellationToken>()).Returns([]);
        using MetricCollector<long> sent = _meters.Collect<long>("signalrhub.notifications.sent");

        Result<SendNotificationResponse> result = await _handler.Handle(new SendNotificationCommand(new ClientTarget(client), "ping", null), CancellationToken.None);

        result.Error.Type.ShouldBe(ErrorType.NotFound);
        result.Error.Code.ShouldBe("Client.NoConnections");
        await _dispatcher.DidNotReceiveWithAnyArgs().SendAsync(default!, default!, default);
        sent.LastMeasurement!.Tags["outcome"].ShouldBe("not_found");
        sent.LastMeasurement.Tags["target"].ShouldBe("client");
    }

    [Fact]
    public async Task Sends_only_to_the_requested_connection()
    {
        ClientConnection connection = TestData.ClientConnection("c1");
        _registry.GetAsync(connection.ConnectionId, Arg.Any<CancellationToken>()).Returns(connection);

        Result<SendNotificationResponse> result = await _handler.Handle(new SendNotificationCommand(new ConnectionTarget(connection.ConnectionId), "ping", null), CancellationToken.None);

        result.Value.DeliveredConnections.ShouldBe(1);
        await _dispatcher.Received(1).SendAsync(
            Arg.Is<IReadOnlyCollection<ConnectionId>>(ids => ids.Single() == connection.ConnectionId),
            Arg.Any<Notification>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Fails_with_not_found_when_connection_does_not_exist()
    {
        ConnectionId connectionId = TestData.Connection("missing");
        _registry.GetAsync(connectionId, Arg.Any<CancellationToken>()).Returns((ClientConnection?)null);

        Result<SendNotificationResponse> result = await _handler.Handle(new SendNotificationCommand(new ConnectionTarget(connectionId), "ping", null), CancellationToken.None);

        result.Error.ShouldBe(ConnectionErrors.ConnectionNotFound(connectionId));
        await _dispatcher.DidNotReceiveWithAnyArgs().SendAsync(default!, default!, default);
    }

    [Fact]
    public async Task Fails_when_no_strategy_supports_the_target()
    {
        Result<SendNotificationResponse> result = await _handler.Handle(new SendNotificationCommand(new UnknownTarget(), "ping", null), CancellationToken.None);

        result.Error.Code.ShouldBe("Notification.UnsupportedTarget");
    }

    public void Dispose() => _meters.Dispose();

    private sealed record UnknownTarget : NotificationTarget;
}
