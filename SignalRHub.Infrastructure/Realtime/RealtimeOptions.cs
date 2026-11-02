namespace SignalRHub.Infrastructure.Realtime;

public sealed class RealtimeOptions
{
    public const string SectionName = "Realtime";

    /// <summary>Route where the SignalR hub is exposed.</summary>
    [Required]
    public string HubPath { get; set; } = "/hubs/notifications";

    /// <summary>Claim of the authenticated principal that holds the client id.</summary>
    [Required]
    public string ClientIdClaim { get; set; } = "sub";
}
