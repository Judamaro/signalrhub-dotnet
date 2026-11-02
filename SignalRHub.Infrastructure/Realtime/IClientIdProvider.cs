namespace SignalRHub.Infrastructure.Realtime;

/// <summary>
/// Extracts the <see cref="ClientId"/> that owns a connection from the authenticated user.
/// </summary>
public interface IClientIdProvider
{
    Result<ClientId> GetClientId(ClaimsPrincipal? user);
}

internal sealed class ClaimClientIdProvider(IOptions<RealtimeOptions> options) : IClientIdProvider
{
    private readonly string _claimType = options.Value.ClientIdClaim;

    public Result<ClientId> GetClientId(ClaimsPrincipal? user)
        => ClientId.Create(user?.FindFirst(_claimType)?.Value);
}
