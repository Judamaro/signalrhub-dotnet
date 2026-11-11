namespace SignalRHub.Application.Abstractions.Connections;

/// <summary>Snapshot of the size of the connection registry.</summary>
/// <param name="Clients">Clients with at least one active connection.</param>
/// <param name="Connections">Active connections.</param>
public readonly record struct ConnectionRegistryStatistics(int Clients, int Connections);
