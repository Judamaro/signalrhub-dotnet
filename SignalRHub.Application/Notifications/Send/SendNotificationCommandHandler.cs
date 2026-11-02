namespace SignalRHub.Application.Notifications.Send;

internal sealed class SendNotificationCommandHandler(IDeliveryStrategyResolver strategyResolver, INotificationDispatcher dispatcher, TimeProvider timeProvider) : ICommandHandler<SendNotificationCommand, SendNotificationResponse>
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
            return recipients.Error;

        await dispatcher.SendAsync(recipients.Value, notification.Value, cancellationToken).ConfigureAwait(false);

        return new SendNotificationResponse(notification.Value.Id, recipients.Value.Count, notification.Value.CreatedAt);
    }
}
