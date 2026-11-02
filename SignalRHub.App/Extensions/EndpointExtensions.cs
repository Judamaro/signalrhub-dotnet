namespace SignalRHub.App.Extensions;

internal static class EndpointExtensions
{
    public static IServiceCollection AddEndpoints(this IServiceCollection services, Assembly assembly)
    {
        IEnumerable<ServiceDescriptor> descriptors = assembly.DefinedTypes
            .Where(t => t is { IsAbstract: false, IsInterface: false } && t.IsAssignableTo(typeof(IEndpoint)))
            .Select(t => ServiceDescriptor.Transient(typeof(IEndpoint), t));

        services.TryAddEnumerable(descriptors);

        return services;
    }

    public static WebApplication MapEndpoints(this WebApplication app)
    {
        foreach (IEndpoint endpoint in app.Services.GetRequiredService<IEnumerable<IEndpoint>>())
        {
            endpoint.MapEndpoint(app);
        }

        return app;
    }
}
