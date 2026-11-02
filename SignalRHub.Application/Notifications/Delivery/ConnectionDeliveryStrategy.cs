namespace SignalRHub.Application.Notifications.Delivery;

/// <summary>Delivers to a single connection, provided it is registered.</summary>
internal sealed class ConnectionDeliveryStrategy(IConnectionRegistry registry) : NotificationDeliveryStrategy<ConnectionTarget>
{
    protected override async Task<Result<IReadOnlyCollection<ConnectionId>>> ResolveRecipientsAsync(ConnectionTarget target, CancellationToken cancellationToken)
    {
        var connection = await registry.GetAsync(target.ConnectionId, cancellationToken).ConfigureAwait(false);

        if (connection is null)
            return Result.Failure<IReadOnlyCollection<ConnectionId>>(ConnectionErrors.ConnectionNotFound(target.ConnectionId));

        return new[] { connection.ConnectionId };
    }
}
