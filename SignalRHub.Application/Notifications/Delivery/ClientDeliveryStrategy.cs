namespace SignalRHub.Application.Notifications.Delivery;

/// <summary>Delivers to every active connection of a client.</summary>
internal sealed class ClientDeliveryStrategy(IConnectionRegistry registry) : NotificationDeliveryStrategy<ClientTarget>
{
    protected override async Task<Result<IReadOnlyCollection<ConnectionId>>> ResolveRecipientsAsync(ClientTarget target, CancellationToken cancellationToken)
    {
        IReadOnlyCollection<ClientConnection> connections = await registry.GetByClientAsync(target.ClientId, cancellationToken).ConfigureAwait(false);

        if (connections.Count == 0)
            return Result.Failure<IReadOnlyCollection<ConnectionId>>(ConnectionErrors.ClientHasNoConnections(target.ClientId));

        return connections.Select(c => c.ConnectionId).ToArray();
    }
}
