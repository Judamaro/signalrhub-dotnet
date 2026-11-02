namespace SignalRHub.App.Extensions;

internal static class OpenApiExtensions
{
    public static IServiceCollection AddHubOpenApi(this IServiceCollection services)
    {
        services.AddOpenApi(options => options.AddDocumentTransformer<ApiKeySecuritySchemeTransformer>());

        return services;
    }

    public static WebApplication UseHubOpenApi(this WebApplication app)
    {
        app.MapOpenApi();
        app.UseSwaggerUI(options =>
        {
            options.SwaggerEndpoint("/openapi/v1.json", "SignalR Hub v1");
            options.DocumentTitle = "SignalR Hub API";
        });

        return app;
    }

    /// <summary>Documents the API key scheme so Swagger UI shows the "Authorize" button.</summary>
    private sealed class ApiKeySecuritySchemeTransformer(IOptions<ApiKeyOptions> apiKeyOptions) : IOpenApiDocumentTransformer
    {
        public Task TransformAsync(OpenApiDocument document, OpenApiDocumentTransformerContext context, CancellationToken cancellationToken)
        {
            document.Components ??= new OpenApiComponents();
            document.Components.SecuritySchemes ??= new Dictionary<string, IOpenApiSecurityScheme>(StringComparer.Ordinal);
            document.Components.SecuritySchemes[ApiKeyAuthenticationHandler.SchemeName] = new OpenApiSecurityScheme
            {
                Type = SecuritySchemeType.ApiKey,
                In = ParameterLocation.Header,
                Name = apiKeyOptions.Value.HeaderName,
                Description = "Key of a backend allowed to publish notifications.",
            };

            document.Security ??= [];
            document.Security.Add(new OpenApiSecurityRequirement
            {
                [new OpenApiSecuritySchemeReference(ApiKeyAuthenticationHandler.SchemeName, document)] = [],
            });

            return Task.CompletedTask;
        }
    }
}
