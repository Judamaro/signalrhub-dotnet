namespace SignalRHub.Application.Notifications.Delivery;

/// <summary>
/// Strategy that knows how to turn a <see cref="NotificationTarget"/> into the concrete connections to notify.
/// </summary>
public interface INotificationDeliveryStrategy
{
    bool CanHandle(NotificationTarget target);

    Task<Result<IReadOnlyCollection<ConnectionId>>> ResolveRecipientsAsync(NotificationTarget target, CancellationToken cancellationToken);
}

/// <summary>
/// Typed base class so each strategy only deals with its own target type.
/// </summary>
public abstract class NotificationDeliveryStrategy<TTarget> : INotificationDeliveryStrategy where TTarget : NotificationTarget
{
    public bool CanHandle(NotificationTarget target) => target is TTarget;

    public Task<Result<IReadOnlyCollection<ConnectionId>>> ResolveRecipientsAsync(NotificationTarget target, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(target);

        return target is TTarget typed ? ResolveRecipientsAsync(typed, cancellationToken) : Task.FromResult(Result.Failure<IReadOnlyCollection<ConnectionId>>(NotificationErrors.UnsupportedTarget(target)));
    }

    protected abstract Task<Result<IReadOnlyCollection<ConnectionId>>> ResolveRecipientsAsync(TTarget target, CancellationToken cancellationToken);
}
