namespace SignalRHub.App.Extensions;

/// <summary>
/// Middleware configuration that keeps every response in the <see cref="PetitionResponse"/> format.
/// </summary>
internal static class PetitionResponseApplicationBuilderExtensions
{
    /// <summary>
    /// Writes a <see cref="PetitionResponse"/> body for error status codes produced without a body,
    /// such as a missing API key (401), unknown routes (404), unsupported methods (405) or media types (415).
    /// </summary>
    public static IApplicationBuilder UsePetitionResponseStatusCodePages(this IApplicationBuilder app)
        => app.UseStatusCodePages(async context =>
        {
            int statusCode = context.HttpContext.Response.StatusCode;
            string message = ReasonPhrases.GetReasonPhrase(statusCode);
            PetitionResponse response = PetitionResponse.Fail(ResponseModules.Api, string.IsNullOrEmpty(message) ? "The request could not be processed." : $"{message}.", statusCode);

            await context.HttpContext.Response.WriteAsJsonAsync(response, context.HttpContext.RequestAborted).ConfigureAwait(false);
        });
}
