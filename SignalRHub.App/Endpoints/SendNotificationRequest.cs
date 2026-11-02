namespace SignalRHub.App.Endpoints;

/// <param name="Type">Event name the clients use to route the message, e.g. "order.updated".</param>
/// <param name="Payload">Arbitrary JSON delivered as-is to the clients.</param>
public sealed record SendNotificationRequest(string Type, JsonElement? Payload);
