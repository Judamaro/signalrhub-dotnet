namespace SignalRHub.App.Extensions;

/// <summary>
/// OpenTelemetry metrics exported in the Prometheus format on a dedicated, internal port.
/// </summary>
internal static class MetricsExtensions
{
    /// <summary>Name reported as <c>service.name</c> (the <c>target_info</c> metric in Prometheus).</summary>
    private const string ServiceName = "signalrhub";

    /// <summary>
    /// Built-in .NET meters: HTTP server, Kestrel, SignalR connections and runtime (GC, CPU, memory, threads).
    /// </summary>
    private static readonly string[] BuiltInMeters =
    [
        "Microsoft.AspNetCore.Hosting",
        "Microsoft.AspNetCore.Server.Kestrel",
        "Microsoft.AspNetCore.Http.Connections",
        "System.Runtime",
    ];

    public static IServiceCollection AddHubMetrics(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddOptions<MetricsOptions>()
            .Bind(configuration.GetSection(MetricsOptions.SectionName))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services.AddOpenTelemetry()
            .ConfigureResource(resource => resource.AddService(ServiceName))
            .WithMetrics(metrics => metrics
                .AddMeter(HubMetrics.MeterName)
                .AddMeter(BuiltInMeters)
                .AddPrometheusExporter());

        return services;
    }

    /// <summary>
    /// Serves the Prometheus scraping endpoint only on <see cref="MetricsOptions.Port"/> and answers <c>404</c> to any
    /// other request on that port. The local port of the connection is checked instead of the Host header, which a
    /// client could forge through the public port.
    /// </summary>
    public static WebApplication UseHubMetrics(this WebApplication app)
    {
        MetricsOptions options = app.Services.GetRequiredService<IOptions<MetricsOptions>>().Value;

        // Create the hub instruments now, so the gauges report 0 before the first connection instead of being absent.
        app.Services.GetRequiredService<HubMetrics>();

        app.UseOpenTelemetryPrometheusScrapingEndpoint(
            meterProvider: null,
            predicate: context => context.Connection.LocalPort == options.Port && context.Request.Path == options.Path,
            path: null,
            configureBranchedPipeline: branch => branch.Use((context, next) =>
            {
                // Scrapes are not application traffic: keep them out of the HTTP server metrics.
                IHttpMetricsTagsFeature? metricsFeature = context.Features.Get<IHttpMetricsTagsFeature>();
                if (metricsFeature is not null)
                    metricsFeature.MetricsDisabled = true;

                return next(context);
            }),
            optionsName: null);

        app.Use((context, next) =>
        {
            if (context.Connection.LocalPort != options.Port)
                return next(context);

            context.Response.StatusCode = StatusCodes.Status404NotFound;

            return Task.CompletedTask;
        });

        return app;
    }
}
