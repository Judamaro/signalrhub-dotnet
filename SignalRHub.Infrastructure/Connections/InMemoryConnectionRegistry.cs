namespace SignalRHub.Infrastructure.Connections;

/// <summary>
/// Single-node connection registry. Keeps a client index and a reverse connection index
/// that are always updated together under one lock, so they never drift apart.
/// Swap it for a distributed implementation (e.g. Redis) to scale out.
/// </summary>
internal sealed class InMemoryConnectionRegistry : IConnectionRegistry
{
    private readonly Lock _gate = new();
    private readonly Dictionary<ClientId, Dictionary<ConnectionId, ClientConnection>> _byClient = [];
    private readonly Dictionary<ConnectionId, ClientConnection> _byConnection = [];

    public Task AddAsync(ClientConnection connection, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(connection);

        lock (_gate)
        {
            RemoveCore(connection.ConnectionId);

            if (!_byClient.TryGetValue(connection.ClientId, out var connections))
            {
                connections = [];
                _byClient[connection.ClientId] = connections;
            }

            connections[connection.ConnectionId] = connection;
            _byConnection[connection.ConnectionId] = connection;
        }

        return Task.CompletedTask;
    }

    public Task<ClientConnection?> RemoveAsync(ConnectionId connectionId, CancellationToken cancellationToken = default)
    {
        lock (_gate)
        {
            return Task.FromResult(RemoveCore(connectionId));
        }
    }

    public Task<ClientConnection?> GetAsync(ConnectionId connectionId, CancellationToken cancellationToken = default)
    {
        lock (_gate)
        {
            return Task.FromResult(_byConnection.GetValueOrDefault(connectionId));
        }
    }

    public Task<IReadOnlyCollection<ClientConnection>> GetByClientAsync(ClientId clientId, CancellationToken cancellationToken = default)
    {
        lock (_gate)
        {
            IReadOnlyCollection<ClientConnection> snapshot = _byClient.TryGetValue(clientId, out var connections) ? connections.Values.ToArray() : [];

            return Task.FromResult(snapshot);
        }
    }

    private ClientConnection? RemoveCore(ConnectionId connectionId)
    {
        if (!_byConnection.Remove(connectionId, out var removed))
            return null;

        if (_byClient.TryGetValue(removed.ClientId, out var connections) && connections.Remove(connectionId) && connections.Count == 0)
            _byClient.Remove(removed.ClientId);

        return removed;
    }
}
