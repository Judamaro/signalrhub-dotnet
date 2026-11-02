namespace SignalRHub.Application.Connections.Unregister;

internal sealed class UnregisterConnectionCommandHandler(IConnectionRegistry registry) : ICommandHandler<UnregisterConnectionCommand, ClientConnectionResponse>
{
    public async Task<Result<ClientConnectionResponse>> Handle(UnregisterConnectionCommand command, CancellationToken cancellationToken)
    {
        var removed = await registry.RemoveAsync(command.ConnectionId, cancellationToken).ConfigureAwait(false);

        return removed is null ? ConnectionErrors.ConnectionNotFound(command.ConnectionId) : ClientConnectionResponse.From(removed);
    }
}
