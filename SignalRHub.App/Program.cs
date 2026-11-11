WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddApplication()
    .AddInfrastructure(builder.Configuration)
    .AddHubAuthentication(builder.Configuration)
    .AddHubOpenApi()
    .AddHubMetrics(builder.Configuration)
    .AddEndpoints(typeof(Program).Assembly);

builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddHealthChecks();

// Binding failures (malformed JSON, missing body) throw so GlobalExceptionHandler answers them with a PetitionResponse.
builder.Services.Configure<RouteHandlerOptions>(options => options.ThrowOnBadRequest = true);

string[] allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [];
builder.Services.AddCors(options => options.AddDefaultPolicy(policy => policy
    .WithOrigins(allowedOrigins)
    .AllowAnyHeader()
    .AllowAnyMethod()
    .AllowCredentials()));

WebApplication app = builder.Build();

app.UseExceptionHandler();
app.UsePetitionResponseStatusCodePages();
app.UseHubMetrics();

if (app.Environment.IsDevelopment())
    app.UseHubOpenApi();

app.UseCors();
app.UseAuthentication();
app.UseAuthorization();

app.MapEndpoints();
app.MapRealtimeHubs();
app.MapHealthEndpoints();

await app.RunAsync().ConfigureAwait(false);

/// <summary>Entry point; public so integration tests can use <c>WebApplicationFactory&lt;Program&gt;</c>.</summary>
public partial class Program;
