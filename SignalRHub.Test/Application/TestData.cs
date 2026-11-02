namespace SignalRHub.Test.Application;

internal static class TestData
{
    public static readonly DateTimeOffset Now = new(2026, 11, 2, 12, 0, 0, TimeSpan.Zero);

    public static ClientId Client(string value = "client-1") => ClientId.Create(value).Value;

    public static ConnectionId Connection(string value) => ConnectionId.Create(value).Value;

    public static ClientConnection ClientConnection(string connectionId, string clientId = "client-1") =>
        new(Connection(connectionId), Client(clientId), Now, UserAgent: null);
}
