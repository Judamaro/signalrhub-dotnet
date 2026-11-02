namespace SignalRHub.Domain.Connections;

/// <summary>
/// Identifies a single realtime connection (one browser tab, device or service instance).
/// </summary>
public readonly record struct ConnectionId
{
    public const int MaxLength = 128;

    private ConnectionId(string value) => Value = value;

    public string Value { get; }

    public static Result<ConnectionId> Create(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return ConnectionErrors.ConnectionIdEmpty;

        string trimmed = value.Trim();
        if (trimmed.Length > MaxLength)
            return ConnectionErrors.ConnectionIdTooLong;

        return new ConnectionId(trimmed);
    }

    public override string ToString() => Value;
}
