namespace SignalRHub.Domain.Notifications;

/// <summary>
/// A message pushed to one or more connections.
/// </summary>
public sealed record Notification
{
    public const int TypeMaxLength = 100;

    private Notification(Guid id, string type, JsonElement? payload, DateTimeOffset createdAt)
    {
        Id = id;
        Type = type;
        Payload = payload;
        CreatedAt = createdAt;
    }

    public Guid Id { get; }

    /// <summary>Event name the clients can use to route the message (e.g. "order.updated").</summary>
    public string Type { get; }

    public JsonElement? Payload { get; }

    public DateTimeOffset CreatedAt { get; }

    public static Result<Notification> Create(string? type, JsonElement? payload, DateTimeOffset createdAt)
    {
        if (string.IsNullOrWhiteSpace(type))
            return NotificationErrors.TypeEmpty;

        string trimmed = type.Trim();
        if (trimmed.Length > TypeMaxLength)
            return NotificationErrors.TypeTooLong;

        return new Notification(Guid.CreateVersion7(createdAt), trimmed, payload?.Clone(), createdAt);
    }
}
