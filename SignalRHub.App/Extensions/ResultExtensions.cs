namespace SignalRHub.App.Extensions;

internal static class ResultExtensions
{
    /// <summary>Maps a failed <see cref="Result"/> to an RFC 9457 problem details response.</summary>
    public static ProblemHttpResult ToProblem(this Result result)
    {
        if (result.IsSuccess)
            throw new InvalidOperationException("A successful result cannot be converted to a problem.");

        Error error = result.Error;
        (int status, string title) = error.Type switch
        {
            ErrorType.Validation => (StatusCodes.Status400BadRequest, "Bad Request"),
            ErrorType.NotFound => (StatusCodes.Status404NotFound, "Not Found"),
            ErrorType.Conflict => (StatusCodes.Status409Conflict, "Conflict"),
            _ => (StatusCodes.Status500InternalServerError, "Server Failure"),
        };

        Dictionary<string, object?> extensions = new(StringComparer.Ordinal) { ["code"] = error.Code };
        if (error is ValidationError validation)
            extensions["errors"] = validation.Errors;

        return TypedResults.Problem(title: title, detail: error.Description, statusCode: status, type: $"https://httpstatuses.io/{status}", extensions: extensions);
    }
}
