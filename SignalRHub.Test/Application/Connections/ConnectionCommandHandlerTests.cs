namespace SignalRHub.Test.Application.Connections;

public sealed class ConnectionCommandHandlerTests : IDisposable
{
    private readonly TestMeterFactory _meters = new();
    private readonly IConnectionRegistry _registry = Substitute.For<IConnectionRegistry>();
    private readonly HubMetrics _metrics;

    public ConnectionCommandHandlerTests()
    {
        _metrics = _meters.CreateHubMetrics(_registry);
    }

    [Fact]
    public async Task Register_adds_the_connection_and_counts_it_as_opened()
    {
        using MetricCollector<long> opened = _meters.Collect<long>("signalrhub.connections.opened");
        RegisterConnectionCommandHandler handler = new(_registry, new FakeTimeProvider(TestData.Now), _metrics);

        Result<ClientConnectionResponse> result = await handler.Handle(new RegisterConnectionCommand(TestData.Connection("c1"), TestData.Client(), "agent"), CancellationToken.None);

        result.Value.ConnectedAt.ShouldBe(TestData.Now);
        await _registry.Received(1).AddAsync(Arg.Is<ClientConnection>(c => c.ConnectionId == TestData.Connection("c1")), Arg.Any<CancellationToken>());
        opened.LastMeasurement!.Value.ShouldBe(1);
    }

    [Fact]
    public async Task Unregister_counts_a_removed_connection_as_closed()
    {
        using MetricCollector<long> closed = _meters.Collect<long>("signalrhub.connections.closed");
        _registry.RemoveAsync(TestData.Connection("c1"), Arg.Any<CancellationToken>()).Returns(TestData.ClientConnection("c1"));
        UnregisterConnectionCommandHandler handler = new(_registry, _metrics);

        Result<ClientConnectionResponse> result = await handler.Handle(new UnregisterConnectionCommand(TestData.Connection("c1")), CancellationToken.None);

        result.IsSuccess.ShouldBeTrue();
        closed.LastMeasurement!.Value.ShouldBe(1);
    }

    [Fact]
    public async Task Unregister_of_an_unknown_connection_is_not_counted()
    {
        using MetricCollector<long> closed = _meters.Collect<long>("signalrhub.connections.closed");
        _registry.RemoveAsync(Arg.Any<ConnectionId>(), Arg.Any<CancellationToken>()).Returns((ClientConnection?)null);
        UnregisterConnectionCommandHandler handler = new(_registry, _metrics);

        Result<ClientConnectionResponse> result = await handler.Handle(new UnregisterConnectionCommand(TestData.Connection("missing")), CancellationToken.None);

        result.Error.Type.ShouldBe(ErrorType.NotFound);
        closed.GetMeasurementSnapshot().ShouldBeEmpty();
    }

    public void Dispose() => _meters.Dispose();
}
