using System.Text.Json.Serialization;
using DotNetEnv;
using Scalar.AspNetCore;
using UniCare.Api.Middleware;
using UniCare.Application;
using UniCare.Infrastructure;

// Load src/UniCare.Api/.env into the environment before configuration is read.
// The file is gitignored; see .env.example for the expected keys.
Env.TraversePath().Load();

var builder = WebApplication.CreateBuilder(args);

const string FrontendCorsPolicy = "frontend";

builder.Services.AddControllers()
    .AddJsonOptions(options =>
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));
builder.Services.AddOpenApi();

// One call per layer. Everything EF Core related lives behind this method, which is
// why this project has no EF Core package reference at all — check the .csproj.
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddApplication();

// Turns domain exceptions into ProblemDetails responses.
builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

// In development the Vite dev server proxies /api to this process, so requests are
// same-origin and CORS never applies. This policy is for deployed environments where
// the SPA is served from a different origin.
var allowedOrigins = builder.Configuration
    .GetSection("Cors:AllowedOrigins")
    .Get<string[]>() ?? [];

builder.Services.AddCors(options =>
{
    options.AddPolicy(FrontendCorsPolicy, policy => policy
        .WithOrigins(allowedOrigins)
        .AllowAnyHeader()
        .AllowAnyMethod()
        .AllowCredentials());
});

var app = builder.Build();

// First in the pipeline — it must wrap everything downstream to catch their exceptions.
app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}
else
{
    // Only redirect in deployed environments; the local http profile has no TLS listener.
    app.UseHttpsRedirection();
}

app.UseCors(FrontendCorsPolicy);

app.UseAuthorization();

app.MapControllers();

app.Run();

// Top-level statements compile to an internal Program class, which
// WebApplicationFactory<Program> in UniCare.Api.IntegrationTests cannot see.
// Declaring it public partial makes the real pipeline testable.
public partial class Program;
