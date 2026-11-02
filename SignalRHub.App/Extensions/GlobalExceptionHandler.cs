namespace SignalRHub.App.Extensions;

/// <summary>
/// Converts unhandled exceptions into <see cref="PetitionResponse"/> responses, hiding internal details from clients.
/// Requests that cannot be bound (e.g. a malformed JSON body) are reported as validation failures.
/// </summary>
internal sealed partial class GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        PetitionResponse response = exception switch
        {
            BadHttpRequestException badRequest => PetitionResponse.Fail(ResponseModules.RequestValidation, "One or more validation errors occurred.", badRequest.StatusCode, new Dictionary<string, object?>(StringComparer.Ordinal) { ["errors"] = new Dictionary<string, string[]>(StringComparer.Ordinal) { ["body"] = [DescribeBadRequest(badRequest)] } }),
            _ => PetitionResponse.Fail(ResponseModules.GlobalExceptionHandler, "An unexpected error occurred.", StatusCodes.Status500InternalServerError),
        };

        if (response.StatusCode >= StatusCodes.Status500InternalServerError)
            LogUnhandled(logger, exception, httpContext.Request.Method, httpContext.Request.Path);
        else
            LogRejected(logger, exception, httpContext.Request.Method, httpContext.Request.Path);

        httpContext.Response.StatusCode = response.StatusCode;
        await httpContext.Response.WriteAsJsonAsync(response, cancellationToken).ConfigureAwait(false);

        return true;
    }

    /// <summary>Describes a binding failure without exposing framework or parameter details to clients.</summary>
    private static string DescribeBadRequest(BadHttpRequestException exception)
        => exception.InnerException is JsonException ? "The request body is not valid JSON." : "The request body is missing or could not be read.";

    [LoggerMessage(Level = LogLevel.Error, Message = "Unhandled exception processing {Method} {Path}")]
    private static partial void LogUnhandled(ILogger logger, Exception exception, string method, PathString path);

    [LoggerMessage(Level = LogLevel.Warning, Message = "Request {Method} {Path} failed")]
    private static partial void LogRejected(ILogger logger, Exception exception, string method, PathString path);
}
