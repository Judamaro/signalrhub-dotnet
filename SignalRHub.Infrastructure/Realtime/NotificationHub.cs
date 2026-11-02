namespace SignalRHub.Infrastructure.Realtime;

/// <summary>
/// Entry point for realtime clients. It only tracks connections; notifications are
/// pushed from the REST API through <see cref="SignalRNotificationDispatcher"/>.
/// </summary>
[Authorize]
public sealed partial class NotificationHub(IClientIdProvider clientIdProvider, ICommandHandler<RegisterConnectionCommand, ClientConnectionResponse> registerConnection,
    ICommandHandler<UnregisterConnectionCommand, ClientConnectionResponse> unregisterConnection, ILogger<NotificationHub> logger) : Hub<INotificationClient>
{
    public override async Task OnConnectedAsync()
    {
        Result<ClientId> clientId = clientIdProvider.GetClientId(Context.User);
        Result<ConnectionId> connectionId = ConnectionId.Create(Context.ConnectionId);

        if (clientId.IsFailure || connectionId.IsFailure)
        {
            LogRejected(logger, Context.ConnectionId);
            Context.Abort();
            return;
        }

        var userAgent = Context.GetHttpContext()?.Request.Headers.UserAgent.ToString();
        RegisterConnectionCommand command = new(connectionId.Value, clientId.Value, string.IsNullOrWhiteSpace(userAgent) ? null : userAgent);

        await registerConnection.Handle(command, Context.ConnectionAborted).ConfigureAwait(false);
        await base.OnConnectedAsync().ConfigureAwait(false);
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        Result<ConnectionId> connectionId = ConnectionId.Create(Context.ConnectionId);
        if (connectionId.IsSuccess)
        {
            await unregisterConnection
                .Handle(new UnregisterConnectionCommand(connectionId.Value), CancellationToken.None)
                .ConfigureAwait(false);
        }

        await base.OnDisconnectedAsync(exception).ConfigureAwait(false);
    }

    /// <summary>Lets a client learn its own connection id (e.g. to share it with its backend).</summary>
    public string GetConnectionId() => Context.ConnectionId;

    [LoggerMessage(Level = LogLevel.Warning, Message = "Connection {ConnectionId} rejected: no valid client id claim")]
    private static partial void LogRejected(ILogger logger, string connectionId);
}
