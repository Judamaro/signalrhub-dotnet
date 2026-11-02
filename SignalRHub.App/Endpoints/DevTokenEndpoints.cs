namespace SignalRHub.App.Endpoints;

/// <summary>
/// Development-only helper that issues client tokens, so the hub can be tried without an identity provider.
/// </summary>
internal sealed class DevTokenEndpoints(IWebHostEnvironment environment) : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        if (!environment.IsDevelopment())
            return;

        app.MapPost("/dev/token", IssueToken)
            .WithTags("Development")
            .WithName("IssueDevelopmentToken")
            .WithSummary("Issues a JWT for a client id (Development only).")
            .ProducesPetitionResponse(StatusCodes.Status200OK, StatusCodes.Status400BadRequest)
            .AllowAnonymous();
    }

    private static IResult IssueToken(DevTokenRequest request, IOptions<JwtOptions> jwtOptions, IOptions<RealtimeOptions> realtimeOptions, TimeProvider timeProvider)
    {
        Result<ClientId> clientId = ClientId.Create(request.ClientId);
        if (clientId.IsFailure)
            return clientId.Error.ToPetitionResponse(ResponseModules.TokenService);

        JwtOptions jwt = jwtOptions.Value;
        DateTimeOffset now = timeProvider.GetUtcNow();
        DateTimeOffset expiresAt = now.AddHours(8);
        SymmetricSecurityKey key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.SigningKey));

        string token = new JsonWebTokenHandler().CreateToken(new SecurityTokenDescriptor
        {
            Issuer = jwt.Issuer,
            Audience = jwt.Audience,
            IssuedAt = now.UtcDateTime,
            NotBefore = now.UtcDateTime,
            Expires = expiresAt.UtcDateTime,
            Subject = new ClaimsIdentity([new Claim(realtimeOptions.Value.ClientIdClaim, clientId.Value.Value)]),
            SigningCredentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256),
        });

        return PetitionResponse.Ok(ResponseModules.TokenService, "Token issued successfully.", new DevTokenResponse(token, expiresAt)).ToHttpResult();
    }
}

public sealed record DevTokenRequest(string ClientId);

public sealed record DevTokenResponse(string AccessToken, DateTimeOffset ExpiresAt);
