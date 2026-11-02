namespace SignalRHub.Application.Connections.Register;

public sealed record RegisterConnectionCommand(ConnectionId ConnectionId, ClientId ClientId, string? UserAgent) : ICommand<ClientConnectionResponse>;
