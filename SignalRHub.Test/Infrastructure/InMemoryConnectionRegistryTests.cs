namespace SignalRHub.Test.Infrastructure;

public sealed class InMemoryConnectionRegistryTests
{
    private static readonly DateTimeOffset Now = new(2026, 11, 2, 12, 0, 0, TimeSpan.Zero);

    private readonly InMemoryConnectionRegistry _registry = new();

    [Fact]
    public async Task Groups_connections_by_client()
    {
        await _registry.AddAsync(Connection("c1", "client-a"));
        await _registry.AddAsync(Connection("c2", "client-a"));
        await _registry.AddAsync(Connection("c3", "client-b"));

        IReadOnlyCollection<ClientConnection> connections = await _registry.GetByClientAsync(Client("client-a"));

        connections.Select(c => c.ConnectionId.Value).Order().ShouldBe(["c1", "c2"]);
    }

    [Fact]
    public async Task Remove_returns_connection_and_drops_it_from_both_indexes()
    {
        await _registry.AddAsync(Connection("c1", "client-a"));

        var removed = await _registry.RemoveAsync(Id("c1"));

        removed.ShouldNotBeNull();
        (await _registry.GetAsync(Id("c1"))).ShouldBeNull();
        (await _registry.GetByClientAsync(Client("client-a"))).ShouldBeEmpty();
    }

    [Fact]
    public async Task Remove_unknown_connection_returns_null()
    {
        (await _registry.RemoveAsync(Id("nope"))).ShouldBeNull();
    }

    [Fact]
    public async Task Re_registering_a_connection_moves_it_to_the_new_client()
    {
        await _registry.AddAsync(Connection("c1", "client-a"));
        await _registry.AddAsync(Connection("c1", "client-b"));

        (await _registry.GetByClientAsync(Client("client-a"))).ShouldBeEmpty();
        (await _registry.GetByClientAsync(Client("client-b"))).Count.ShouldBe(1);
    }

    [Fact]
    public async Task Handles_concurrent_adds_and_removes()
    {
        string[] ids = Enumerable.Range(0, 1_000).Select(i => $"c{i}").ToArray();

        await Parallel.ForEachAsync(ids, async (id, ct) => await _registry.AddAsync(Connection(id, "client-a"), ct));
        (await _registry.GetByClientAsync(Client("client-a"))).Count.ShouldBe(ids.Length);

        await Parallel.ForEachAsync(ids, async (id, ct) => await _registry.RemoveAsync(Id(id), ct));
        (await _registry.GetByClientAsync(Client("client-a"))).ShouldBeEmpty();
    }

    private static ClientId Client(string value) => ClientId.Create(value).Value;

    private static ConnectionId Id(string value) => ConnectionId.Create(value).Value;

    private static ClientConnection Connection(string connectionId, string clientId) =>
        new(Id(connectionId), Client(clientId), Now, UserAgent: null);
}
