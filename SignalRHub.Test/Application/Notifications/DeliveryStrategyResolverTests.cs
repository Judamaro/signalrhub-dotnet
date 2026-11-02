namespace SignalRHub.Test.Application.Notifications;

public sealed class DeliveryStrategyResolverTests
{
    private readonly DeliveryStrategyResolver _resolver;

    public DeliveryStrategyResolverTests()
    {
        IConnectionRegistry registry = Substitute.For<IConnectionRegistry>();
        _resolver = new DeliveryStrategyResolver([new ClientDeliveryStrategy(registry), new ConnectionDeliveryStrategy(registry)]);
    }

    [Fact]
    public void Resolves_client_strategy_for_client_target()
    {
        _resolver.Resolve(new ClientTarget(TestData.Client())).Value.ShouldBeOfType<ClientDeliveryStrategy>();
    }

    [Fact]
    public void Resolves_connection_strategy_for_connection_target()
    {
        _resolver.Resolve(new ConnectionTarget(TestData.Connection("c1"))).Value.ShouldBeOfType<ConnectionDeliveryStrategy>();
    }
}
