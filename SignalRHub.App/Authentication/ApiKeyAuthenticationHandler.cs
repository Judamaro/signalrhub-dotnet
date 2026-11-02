namespace SignalRHub.App.Authentication;

/// <summary>
/// Authenticates server-to-server callers through a shared key sent in a header.
/// </summary>
internal sealed class ApiKeyAuthenticationHandler(IOptionsMonitor<AuthenticationSchemeOptions> schemeOptions, IOptions<ApiKeyOptions> apiKeyOptions, ILoggerFactory loggerFactory, UrlEncoder encoder)
 : AuthenticationHandler<AuthenticationSchemeOptions>(schemeOptions, loggerFactory, encoder)
{
    public const string SchemeName = "ApiKey";

    private readonly ApiKeyOptions _apiKeys = apiKeyOptions.Value;

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        if (!Request.Headers.TryGetValue(_apiKeys.HeaderName, out StringValues values))
            return Task.FromResult(AuthenticateResult.NoResult());

        string provided = values.ToString();
        if (string.IsNullOrEmpty(provided) || !IsValid(provided))
            return Task.FromResult(AuthenticateResult.Fail("Invalid API key."));

        ClaimsIdentity identity = new ClaimsIdentity([new Claim(ClaimTypes.Name, "notification-publisher")], SchemeName);
        AuthenticationTicket ticket = new AuthenticationTicket(new ClaimsPrincipal(identity), SchemeName);

        return Task.FromResult(AuthenticateResult.Success(ticket));
    }

    private bool IsValid(string provided)
    {
        byte[] providedBytes = Encoding.UTF8.GetBytes(provided);
        bool match = false;

        foreach (string key in _apiKeys.Keys)
        {
            match |= CryptographicOperations.FixedTimeEquals(providedBytes, Encoding.UTF8.GetBytes(key));
        }

        return match;
    }
}
