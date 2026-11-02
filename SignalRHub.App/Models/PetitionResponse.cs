namespace SignalRHub.App.Models;

/// <summary>
/// Envelope returned by every endpoint of the API.
/// </summary>
public sealed record PetitionResponse
{
    /// <summary>Gets a value indicating whether the petition was processed successfully.</summary>
    public bool Success { get; init; }

    /// <summary>Gets a message describing how the petition was processed.</summary>
    public required string Message { get; init; }

    /// <summary>Gets the module or service that generated <see cref="Message"/>.</summary>
    public required string Module { get; init; }

    /// <summary>
    /// Gets the information returned by the petition, or <see langword="null"/> when there is none.
    /// On failures it holds additional error details, such as validation errors, when available.
    /// </summary>
    public object? Result { get; init; }

    /// <summary>Gets the HTTP status code of the petition, also used as the status code of the response.</summary>
    public int StatusCode { get; init; }

    /// <summary>Creates a successful petition response.</summary>
    public static PetitionResponse Ok(string module, string message, object? result = null, int statusCode = StatusCodes.Status200OK)
        => new() { Success = true, Message = message, Module = module, Result = result, StatusCode = statusCode, };

    /// <summary>Creates a failed petition response.</summary>
    public static PetitionResponse Fail(string module, string message, int statusCode, object? result = null)
        => new() { Success = false, Message = message, Module = module, Result = result, StatusCode = statusCode, };

    /// <summary>Converts the petition response into an HTTP result whose status code is <see cref="StatusCode"/>.</summary>
    public IResult ToHttpResult() => TypedResults.Json(this, statusCode: StatusCode);
}
