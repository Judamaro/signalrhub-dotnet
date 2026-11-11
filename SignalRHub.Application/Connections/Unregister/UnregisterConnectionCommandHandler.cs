namespace SignalRHub.Application.Connections.Unregister;

internal sealed class UnregisterConnectionCommandHandler(IConnectionRegistry registry, HubMetrics metrics) : ICommandHandler<UnregisterConnectionCommand, ClientConnectionResponse>
{
    public async Task<Result<ClientConnectionResponse>> Handle(UnregisterConnectionCommand command, CancellationToken cancellationToken)
    {
        var removed = await registry.RemoveAsync(command.ConnectionId, cancellationToken).ConfigureAwait(false);

        if (removed is null)
            return ConnectionErrors.ConnectionNotFound(command.ConnectionId);

        metrics.ConnectionClosed();

        return ClientConnectionResponse.From(removed);
    }
}
