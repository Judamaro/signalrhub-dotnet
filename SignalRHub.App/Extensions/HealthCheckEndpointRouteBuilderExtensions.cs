namespace SignalRHub.App.Extensions;

/// <summary>
/// Maps the health check endpoint, which also answers with a <see cref="PetitionResponse"/>.
/// </summary>
internal static class HealthCheckEndpointRouteBuilderExtensions
{
    /// <summary>Path of the liveness endpoint, used by the Docker healthcheck.</summary>
    public const string LivenessPath = "/health";

    /// <summary>
    /// Maps <c>/health</c> (liveness: the API is running). Healthy and degraded reports answer <c>200</c>,
    /// unhealthy reports answer <c>503</c>.
    /// </summary>
    public static IEndpointRouteBuilder MapHealthEndpoints(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapHealthChecks(LivenessPath, new HealthCheckOptions { ResponseWriter = WritePetitionResponseAsync });

        return endpoints;
    }

    /// <summary>
    /// Writes the health report as a <see cref="PetitionResponse"/> whose result holds the status of each check.
    /// The status code was already set by the health check middleware from the report status.
    /// </summary>
    private static Task WritePetitionResponseAsync(HttpContext context, HealthReport report)
    {
        Dictionary<string, object?> checks = report.Entries.ToDictionary(
            entry => entry.Key,
            entry => (object?)new
            {
                Status = entry.Value.Status.ToString(),
                entry.Value.Description,
                DurationMs = Math.Round(entry.Value.Duration.TotalMilliseconds, 2),
            },
            StringComparer.Ordinal);

        object result = new { Status = report.Status.ToString(), Checks = checks };
        int statusCode = context.Response.StatusCode;
        string message = $"The service is {report.Status.ToString().ToLowerInvariant()}.";

        PetitionResponse response = report.Status == HealthStatus.Unhealthy ? PetitionResponse.Fail(ResponseModules.HealthCheck, message, statusCode, result) : PetitionResponse.Ok(ResponseModules.HealthCheck, message, result, statusCode);

        return context.Response.WriteAsJsonAsync(response, context.RequestAborted);
    }
}
