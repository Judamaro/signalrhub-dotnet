namespace SignalRHub.App.Models;

/// <summary>
/// Names reported in <see cref="PetitionResponse.Module"/>.
/// </summary>
public static class ResponseModules
{
    /// <summary>Notification use cases (send to a client or to a connection).</summary>
    public const string NotificationService = "NotificationService";

    /// <summary>Connection use cases (list the connections of a client).</summary>
    public const string ConnectionService = "ConnectionService";

    /// <summary>Development token issuing.</summary>
    public const string TokenService = "TokenService";

    /// <summary>Validation of the request format performed before reaching a service.</summary>
    public const string RequestValidation = "RequestValidation";

    /// <summary>Handler of unexpected exceptions.</summary>
    public const string GlobalExceptionHandler = "GlobalExceptionHandler";

    /// <summary>Health check endpoints.</summary>
    public const string HealthCheck = "HealthCheck";

    /// <summary>HTTP pipeline (unknown routes, unsupported methods or media types, authentication).</summary>
    public const string Api = "Api";
}
