namespace SignalRHub.App.Endpoints;

internal sealed class NotificationEndpoints : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        RouteGroupBuilder group = app.MapGroup("/api/notifications")
            .WithTags("Notifications")
            .RequireAuthorization(AuthPolicies.NotificationPublisher);

        group.MapPost("/clients/{clientId}", SendToClient)
            .WithName("SendNotificationToClient")
            .WithSummary("Sends a notification to every active connection of a client.");

        group.MapPost("/connections/{connectionId}", SendToConnection)
            .WithName("SendNotificationToConnection")
            .WithSummary("Sends a notification to a single connection.");
    }

    private static Task<Results<Ok<SendNotificationResponse>, ProblemHttpResult>> SendToClient(string clientId, SendNotificationRequest request, ICommandHandler<SendNotificationCommand, SendNotificationResponse> handler, CancellationToken cancellationToken)
    {
        Result<ClientId> id = ClientId.Create(clientId);

        return id.IsFailure ? Task.FromResult<Results<Ok<SendNotificationResponse>, ProblemHttpResult>>(id.ToProblem()) : Send(new ClientTarget(id.Value), request, handler, cancellationToken);
    }

    private static Task<Results<Ok<SendNotificationResponse>, ProblemHttpResult>> SendToConnection(string connectionId, SendNotificationRequest request, ICommandHandler<SendNotificationCommand, SendNotificationResponse> handler, CancellationToken cancellationToken)
    {
        Result<ConnectionId> id = ConnectionId.Create(connectionId);

        return id.IsFailure ? Task.FromResult<Results<Ok<SendNotificationResponse>, ProblemHttpResult>>(id.ToProblem()) : Send(new ConnectionTarget(id.Value), request, handler, cancellationToken);
    }

    private static async Task<Results<Ok<SendNotificationResponse>, ProblemHttpResult>> Send(NotificationTarget target, SendNotificationRequest request, ICommandHandler<SendNotificationCommand, SendNotificationResponse> handler, CancellationToken cancellationToken)
    {
        Result<SendNotificationResponse> result = await handler.Handle(new SendNotificationCommand(target, request.Type, request.Payload), cancellationToken).ConfigureAwait(false);

        return result.IsSuccess ? TypedResults.Ok(result.Value) : result.ToProblem();
    }
}
