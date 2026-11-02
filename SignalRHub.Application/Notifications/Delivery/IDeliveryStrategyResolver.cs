namespace SignalRHub.Application.Notifications.Delivery;

/// <summary>
/// Factory that selects the delivery strategy for a given target.
/// </summary>
public interface IDeliveryStrategyResolver
{
    Result<INotificationDeliveryStrategy> Resolve(NotificationTarget target);
}

internal sealed class DeliveryStrategyResolver(IEnumerable<INotificationDeliveryStrategy> strategies) : IDeliveryStrategyResolver
{
    private readonly INotificationDeliveryStrategy[] _strategies = strategies.ToArray();

    public Result<INotificationDeliveryStrategy> Resolve(NotificationTarget target)
    {
        ArgumentNullException.ThrowIfNull(target);

        var strategy = Array.Find(_strategies, s => s.CanHandle(target));

        return strategy is null ? Result.Failure<INotificationDeliveryStrategy>(NotificationErrors.UnsupportedTarget(target)) : Result.Success(strategy);
    }
}
