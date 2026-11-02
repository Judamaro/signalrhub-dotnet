namespace SignalRHub.Application.Connections;

public sealed record ClientConnectionResponse(
    string ConnectionId,
    string ClientId,
    DateTimeOffset ConnectedAt,
    string? UserAgent)
{
    public static ClientConnectionResponse From(ClientConnection connection) => new(
        connection.ConnectionId.Value,
        connection.ClientId.Value,
        connection.ConnectedAt,
        connection.UserAgent);
}
