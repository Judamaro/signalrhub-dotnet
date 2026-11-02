namespace SignalRHub.Application.Connections.GetClientConnections;

internal sealed class GetClientConnectionsQueryHandler(IConnectionRegistry registry) : IQueryHandler<GetClientConnectionsQuery, IReadOnlyCollection<ClientConnectionResponse>>
{
    public async Task<Result<IReadOnlyCollection<ClientConnectionResponse>>> Handle(GetClientConnectionsQuery query, CancellationToken cancellationToken)
    {
        IReadOnlyCollection<ClientConnection> connections = await registry.GetByClientAsync(query.ClientId, cancellationToken).ConfigureAwait(false);

        return connections
            .OrderBy(c => c.ConnectedAt)
            .Select(ClientConnectionResponse.From)
            .ToArray();
    }
}
