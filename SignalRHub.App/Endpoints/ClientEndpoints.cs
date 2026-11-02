namespace SignalRHub.App.Endpoints;

internal sealed class ClientEndpoints : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGroup("/api/clients")
            .WithTags("Clients")
            .RequireAuthorization(AuthPolicies.NotificationPublisher)
            .MapGet("/{clientId}/connections", GetConnections)
            .WithName("GetClientConnections")
            .WithSummary("Lists the active connections of a client.")
            .ProducesPetitionResponse(StatusCodes.Status200OK, StatusCodes.Status400BadRequest, StatusCodes.Status401Unauthorized);
    }

    private static async Task<IResult> GetConnections(string clientId, IQueryHandler<GetClientConnectionsQuery, IReadOnlyCollection<ClientConnectionResponse>> handler, CancellationToken cancellationToken)
    {
        Result<ClientId> id = ClientId.Create(clientId);
        if (id.IsFailure)
            return id.Error.ToPetitionResponse(ResponseModules.ConnectionService);

        Result<IReadOnlyCollection<ClientConnectionResponse>> result = await handler.Handle(new GetClientConnectionsQuery(id.Value), cancellationToken).ConfigureAwait(false);

        return result.ToPetitionResponse(ResponseModules.ConnectionService, "Connections retrieved successfully.");
    }
}
