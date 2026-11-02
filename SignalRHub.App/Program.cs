WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddApplication()
    .AddInfrastructure(builder.Configuration)
    .AddHubAuthentication(builder.Configuration)
    .AddHubOpenApi()
    .AddEndpoints(typeof(Program).Assembly);

builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddHealthChecks();

string[] allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [];
builder.Services.AddCors(options => options.AddDefaultPolicy(policy => policy
    .WithOrigins(allowedOrigins)
    .AllowAnyHeader()
    .AllowAnyMethod()
    .AllowCredentials()));

WebApplication app = builder.Build();

app.UseExceptionHandler();
app.UseStatusCodePages();

if (app.Environment.IsDevelopment())
    app.UseHubOpenApi();

app.UseCors();
app.UseAuthentication();
app.UseAuthorization();

app.MapEndpoints();
app.MapRealtimeHubs();
app.MapHealthChecks("/health");

await app.RunAsync().ConfigureAwait(false);

/// <summary>Entry point; public so integration tests can use <c>WebApplicationFactory&lt;Program&gt;</c>.</summary>
public partial class Program;
