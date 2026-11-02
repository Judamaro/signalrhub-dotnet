namespace SignalRHub.Domain.Connections;

public static class ConnectionErrors
{
    public static readonly Error ClientIdEmpty = Error.Validation("ClientId.Empty", "The client id is required.");

    public static readonly Error ClientIdTooLong = Error.Validation("ClientId.TooLong", $"The client id cannot exceed {ClientId.MaxLength} characters.");

    public static readonly Error ConnectionIdEmpty = Error.Validation("ConnectionId.Empty", "The connection id is required.");

    public static readonly Error ConnectionIdTooLong = Error.Validation("ConnectionId.TooLong", $"The connection id cannot exceed {ConnectionId.MaxLength} characters.");

    public static Error ClientHasNoConnections(ClientId clientId)
        => Error.NotFound("Client.NoConnections", $"The client '{clientId}' has no active connections.");

    public static Error ConnectionNotFound(ConnectionId connectionId)
        => Error.NotFound("Connection.NotFound", $"The connection '{connectionId}' was not found.");
}
