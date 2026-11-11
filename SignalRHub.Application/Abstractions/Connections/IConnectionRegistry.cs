namespace SignalRHub.Application.Abstractions.Connections;

/// <summary>
/// Repository of the active connections, indexed by client.
/// Implementations may be in-memory (single node) or distributed (e.g. Redis).
/// </summary>
public interface IConnectionRegistry
{
    Task AddAsync(ClientConnection connection, CancellationToken cancellationToken = default);

    /// <returns>The removed connection, or <see langword="null"/> when it was not registered.</returns>
    Task<ClientConnection?> RemoveAsync(ConnectionId connectionId, CancellationToken cancellationToken = default);

    Task<ClientConnection?> GetAsync(ConnectionId connectionId, CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<ClientConnection>> GetByClientAsync(ClientId clientId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns how many clients and connections are registered. It is synchronous and must be cheap,
    /// because metrics read it on every scrape.
    /// </summary>
    ConnectionRegistryStatistics GetStatistics();
}
