namespace SignalRHub.Application.Connections.GetClientConnections;

public sealed record GetClientConnectionsQuery(ClientId ClientId) : IQuery<IReadOnlyCollection<ClientConnectionResponse>>;
