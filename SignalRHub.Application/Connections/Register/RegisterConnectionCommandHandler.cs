namespace SignalRHub.Application.Connections.Register;

internal sealed class RegisterConnectionCommandHandler(IConnectionRegistry registry, TimeProvider timeProvider) : ICommandHandler<RegisterConnectionCommand, ClientConnectionResponse>
{
    public async Task<Result<ClientConnectionResponse>> Handle(RegisterConnectionCommand command, CancellationToken cancellationToken)
    {
        ClientConnection connection = new(command.ConnectionId, command.ClientId, timeProvider.GetUtcNow(), command.UserAgent);

        await registry.AddAsync(connection, cancellationToken).ConfigureAwait(false);

        return ClientConnectionResponse.From(connection);
    }
}
