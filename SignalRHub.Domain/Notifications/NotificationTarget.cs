namespace SignalRHub.Domain.Notifications;

/// <summary>
/// Who should receive a notification. New kinds of targets (groups, broadcast, ...)
/// are added as new subtypes plus a matching delivery strategy.
/// </summary>
public abstract record NotificationTarget;

/// <summary>Every active connection of a client.</summary>
public sealed record ClientTarget(ClientId ClientId) : NotificationTarget;

/// <summary>A single, specific connection.</summary>
public sealed record ConnectionTarget(ConnectionId ConnectionId) : NotificationTarget;
