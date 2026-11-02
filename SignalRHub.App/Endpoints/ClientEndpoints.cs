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
            .WithSummary("Lists the active connections of a client.");
    }

    private static async Task<Results<Ok<IReadOnlyCollection<ClientConnectionResponse>>, ProblemHttpResult>> GetConnections(string clientId, IQueryHandler<GetClientConnectionsQuery, IReadOnlyCollection<ClientConnectionResponse>> handler, CancellationToken cancellationToken)
    {
        Result<ClientId> id = ClientId.Create(clientId);
        if (id.IsFailure)
            return id.ToProblem();

        Result<IReadOnlyCollection<ClientConnectionResponse>> result = await handler.Handle(new GetClientConnectionsQuery(id.Value), cancellationToken).ConfigureAwait(false);

        return result.IsSuccess ? TypedResults.Ok(result.Value) : result.ToProblem();
    }
}
