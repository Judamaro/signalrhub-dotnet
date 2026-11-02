namespace SignalRHub.App.Endpoints;

internal sealed class NotificationEndpoints : IEndpoint
{
    private const string SuccessMessage = "Notification sent successfully.";

    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        RouteGroupBuilder group = app.MapGroup("/api/notifications")
            .WithTags("Notifications")
            .RequireAuthorization(AuthPolicies.NotificationPublisher);

        group.MapPost("/clients/{clientId}", SendToClient)
            .WithName("SendNotificationToClient")
            .WithSummary("Sends a notification to every active connection of a client.")
            .ProducesPetitionResponse(StatusCodes.Status200OK, StatusCodes.Status400BadRequest, StatusCodes.Status401Unauthorized, StatusCodes.Status404NotFound);

        group.MapPost("/connections/{connectionId}", SendToConnection)
            .WithName("SendNotificationToConnection")
            .WithSummary("Sends a notification to a single connection.")
            .ProducesPetitionResponse(StatusCodes.Status200OK, StatusCodes.Status400BadRequest, StatusCodes.Status401Unauthorized, StatusCodes.Status404NotFound);
    }

    private static Task<IResult> SendToClient(string clientId, SendNotificationRequest request, ICommandHandler<SendNotificationCommand, SendNotificationResponse> handler, CancellationToken cancellationToken)
    {
        Result<ClientId> id = ClientId.Create(clientId);

        return id.IsFailure ? Task.FromResult(id.Error.ToPetitionResponse(ResponseModules.NotificationService)) : Send(new ClientTarget(id.Value), request, handler, cancellationToken);
    }

    private static Task<IResult> SendToConnection(string connectionId, SendNotificationRequest request, ICommandHandler<SendNotificationCommand, SendNotificationResponse> handler, CancellationToken cancellationToken)
    {
        Result<ConnectionId> id = ConnectionId.Create(connectionId);

        return id.IsFailure ? Task.FromResult(id.Error.ToPetitionResponse(ResponseModules.NotificationService)) : Send(new ConnectionTarget(id.Value), request, handler, cancellationToken);
    }

    private static async Task<IResult> Send(NotificationTarget target, SendNotificationRequest request, ICommandHandler<SendNotificationCommand, SendNotificationResponse> handler, CancellationToken cancellationToken)
    {
        Result<SendNotificationResponse> result = await handler.Handle(new SendNotificationCommand(target, request.Type, request.Payload), cancellationToken).ConfigureAwait(false);

        return result.ToPetitionResponse(ResponseModules.NotificationService, SuccessMessage);
    }
}
