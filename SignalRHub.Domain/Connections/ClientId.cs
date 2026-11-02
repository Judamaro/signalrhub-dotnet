namespace SignalRHub.Domain.Connections;

/// <summary>
/// Identifies the client (tenant, user or application) that owns one or more connections.
/// </summary>
public readonly record struct ClientId
{
    public const int MaxLength = 128;

    private ClientId(string value) => Value = value;

    public string Value { get; }

    public static Result<ClientId> Create(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return ConnectionErrors.ClientIdEmpty;

        string trimmed = value.Trim();
        if (trimmed.Length > MaxLength)
            return ConnectionErrors.ClientIdTooLong;

        return new ClientId(trimmed);
    }

    public override string ToString() => Value;
}
