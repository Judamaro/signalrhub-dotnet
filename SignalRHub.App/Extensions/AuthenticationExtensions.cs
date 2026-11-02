namespace SignalRHub.App.Extensions;

internal static class AuthenticationExtensions
{
    public static IServiceCollection AddHubAuthentication(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddOptions<JwtOptions>()
            .Bind(configuration.GetSection(JwtOptions.SectionName))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services.AddOptions<ApiKeyOptions>()
            .Bind(configuration.GetSection(ApiKeyOptions.SectionName))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services
            .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer()
            .AddScheme<AuthenticationSchemeOptions, ApiKeyAuthenticationHandler>(ApiKeyAuthenticationHandler.SchemeName, null);

        services.AddOptions<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme)
            .Configure<IOptions<JwtOptions>, IOptions<RealtimeOptions>>((bearer, jwt, realtime) =>
            {
                bearer.MapInboundClaims = false;
                bearer.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidIssuer = jwt.Value.Issuer,
                    ValidAudience = jwt.Value.Audience,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.Value.SigningKey)),
                    ClockSkew = TimeSpan.FromSeconds(30),
                };

                string hubPath = realtime.Value.HubPath;
                bearer.Events = new JwtBearerEvents
                {
                    OnMessageReceived = context =>
                    {
                        StringValues accessToken = context.Request.Query["access_token"];
                        if (!string.IsNullOrEmpty(accessToken) && context.HttpContext.Request.Path.StartsWithSegments(hubPath))
                        {
                            context.Token = accessToken;
                        }

                        return Task.CompletedTask;
                    },
                };
            });

        services.AddAuthorizationBuilder()
            .AddPolicy(AuthPolicies.NotificationPublisher, policy => policy
                .AddAuthenticationSchemes(ApiKeyAuthenticationHandler.SchemeName)
                .RequireAuthenticatedUser());

        return services;
    }
}
