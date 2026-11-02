namespace SignalRHub.Application.Connections.Unregister;

public sealed record UnregisterConnectionCommand(ConnectionId ConnectionId) : ICommand<ClientConnectionResponse>;
