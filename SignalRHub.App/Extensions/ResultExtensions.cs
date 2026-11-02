namespace SignalRHub.App.Extensions;

/// <summary>
/// Translates <see cref="Result"/> values into <see cref="PetitionResponse"/> responses.
/// </summary>
internal static class ResultExtensions
{
    /// <summary>Creates the response of an operation that returns a value.</summary>
    public static IResult ToPetitionResponse<TValue>(this Result<TValue> result, string module, string successMessage, int successStatusCode = StatusCodes.Status200OK)
        => result.IsSuccess ? PetitionResponse.Ok(module, successMessage, result.Value, successStatusCode).ToHttpResult() : result.Error.ToPetitionResponse(module);

    /// <summary>Creates the response of a failed operation. Validation errors, grouped by field, are returned as the result.</summary>
    public static IResult ToPetitionResponse(this Error error, string module)
    {
        int statusCode = error.Type switch
        {
            ErrorType.Validation => StatusCodes.Status400BadRequest,
            ErrorType.NotFound => StatusCodes.Status404NotFound,
            ErrorType.Conflict => StatusCodes.Status409Conflict,
            _ => StatusCodes.Status500InternalServerError,
        };

        Dictionary<string, object?>? details = error is ValidationError validation ? new Dictionary<string, object?>(StringComparer.Ordinal) { ["errors"] = validation.Errors } : null;

        return PetitionResponse.Fail(module, error.Description, statusCode, details).ToHttpResult();
    }

    /// <summary>Documents that the endpoint answers with a <see cref="PetitionResponse"/> for each status code.</summary>
    public static RouteHandlerBuilder ProducesPetitionResponse(this RouteHandlerBuilder builder, params int[] statusCodes)
    {
        foreach (int statusCode in statusCodes)
            builder.Produces<PetitionResponse>(statusCode);

        return builder;
    }
}
