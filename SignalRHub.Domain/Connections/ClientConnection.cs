namespace SignalRHub.Domain.Connections;

/// <summary>
/// An active connection that belongs to a client.
/// </summary>
public sealed record ClientConnection(ConnectionId ConnectionId, ClientId ClientId, DateTimeOffset ConnectedAt, string? UserAgent);
