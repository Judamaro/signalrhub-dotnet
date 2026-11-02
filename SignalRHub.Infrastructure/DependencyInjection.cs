namespace SignalRHub.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddOptions<RealtimeOptions>()
            .Bind(configuration.GetSection(RealtimeOptions.SectionName))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services.AddSignalR();

        services.AddSingleton<IConnectionRegistry, InMemoryConnectionRegistry>();
        services.AddSingleton<INotificationDispatcher, SignalRNotificationDispatcher>();
        services.AddSingleton<IClientIdProvider, ClaimClientIdProvider>();

        return services;
    }

    public static IEndpointRouteBuilder MapRealtimeHubs(this IEndpointRouteBuilder endpoints)
    {
        ArgumentNullException.ThrowIfNull(endpoints);

        RealtimeOptions options = endpoints.ServiceProvider.GetRequiredService<IOptions<RealtimeOptions>>().Value;
        endpoints.MapHub<NotificationHub>(options.HubPath);

        return endpoints;
    }
}
