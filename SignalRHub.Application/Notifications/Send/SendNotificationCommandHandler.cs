namespace SignalRHub.Application.Notifications.Send;

internal sealed class SendNotificationCommandHandler(IDeliveryStrategyResolver strategyResolver, INotificationDispatcher dispatcher, TimeProvider timeProvider, HubMetrics metrics) : ICommandHandler<SendNotificationCommand, SendNotificationResponse>
{
    public async Task<Result<SendNotificationResponse>> Handle(SendNotificationCommand command, CancellationToken cancellationToken)
    {
        Result<Notification> notification = Notification.Create(command.Type, command.Payload, timeProvider.GetUtcNow());
        if (notification.IsFailure)
            return notification.Error;

        Result<INotificationDeliveryStrategy> strategy = strategyResolver.Resolve(command.Target);
        if (strategy.IsFailure)
            return strategy.Error;

        Result<IReadOnlyCollection<ConnectionId>> recipients = await strategy.Value.ResolveRecipientsAsync(command.Target, cancellationToken).ConfigureAwait(false);
        if (recipients.IsFailure)
        {
            if (recipients.Error.Type == ErrorType.NotFound)
                metrics.NotificationNotFound(command.Target);

            return recipients.Error;
        }

        long dispatchStart = timeProvider.GetTimestamp();
        await dispatcher.SendAsync(recipients.Value, notification.Value, cancellationToken).ConfigureAwait(false);
        metrics.NotificationDelivered(command.Target, recipients.Value.Count, timeProvider.GetElapsedTime(dispatchStart));

        return new SendNotificationResponse(notification.Value.Id, recipients.Value.Count, notification.Value.CreatedAt);
    }
}
