using System.IO.Compression;
using Blogify.Api.Controllers;
using Blogify.Api.Extensions;
using Blogify.Api.RateLimiting;
using Blogify.Application;
using Blogify.Infrastructure;
using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.OpenApi;
using Scalar.AspNetCore;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.AddOpenTelemetryMonitoring("Blogify.Api", ApiVersions.V1.ToString());

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

builder.Services.AddControllers();

builder.Services.AddBlogifyForwardedHeaders(builder.Configuration);
builder.Services.AddBlogifyRateLimiting(builder.Configuration);

var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [];

builder.Services.AddCors(options =>
{
    options.AddPolicy("DefaultCorsPolicy", policy =>
    {
        if (allowedOrigins.Length > 0)
            policy.WithOrigins(allowedOrigins).AllowAnyHeader().AllowAnyMethod();
        else if (builder.Environment.IsDevelopment())
            policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod(); // local convenience only
        // Outside Development with no configured origins, cross-origin requests are not allowed.
    });
});

builder.Services.AddResponseCompression(options =>
{
    options.EnableForHttps = true;
    options.Providers.Add<BrotliCompressionProvider>();
    options.Providers.Add<GzipCompressionProvider>();
});
builder.Services.Configure<BrotliCompressionProviderOptions>(options => options.Level = CompressionLevel.Optimal);
builder.Services.Configure<GzipCompressionProviderOptions>(options => options.Level = CompressionLevel.Optimal);

builder.Services.AddOpenApi(options =>
{
    options.AddDocumentTransformer((document, _, _) =>
    {
        document.Info = new OpenApiInfo
        {
            Title = "Blogify API",
            Version = "v1",
            Description = "API for Blogify platform.",
            Contact = new OpenApiContact
            {
                Name = "Blogify Dev Team",
                Email = "devteam@blogify.com",
                Url = new Uri("https://blogify.com")
            },
            License = new OpenApiLicense
            {
                Name = "MIT License (Blogify)",
                Url = new Uri("https://opensource.org/licenses/MIT")
            }
        };
        return Task.CompletedTask;
    });
});

var app = builder.Build();

// Apply DB migrations on startup if enabled (with retry for container orchestration race conditions)
var migrateOpt = builder.Configuration.GetSection("Database");
if (migrateOpt.GetValue<bool>("MigrateOnStartup"))
{
    var maxRetries = migrateOpt.GetValue<int?>("MigrationMaxRetries") ?? 5;
    var baseDelay = TimeSpan.FromSeconds(migrateOpt.GetValue<int?>("MigrationBaseDelaySeconds") ?? 2);
    var attempt = 0;
    while (true)
    {
        try
        {
            app.ApplyMigrations();
            Log.Information("Database migrations applied successfully on startup (attempt {Attempt})", attempt + 1);
            break;
        }
        catch (Exception ex)
        {
            attempt++;
            if (attempt >= maxRetries)
            {
                // Fail-fast: don't let the application start with an out-of-date / invalid schema
                Log.Fatal(ex, "Failed to apply database migrations after {Attempts} attempts. Application is shutting down.", attempt);
                throw; // Rethrow to abort startup
            }
            var delay = TimeSpan.FromMilliseconds(baseDelay.TotalMilliseconds * Math.Pow(2, attempt - 1));
            Log.Warning(ex, "Migration attempt {Attempt} failed. Retrying in {Delay}...", attempt, delay);
            await Task.Delay(delay);
        }
    }
}

// First in the pipeline: everything after it (exception handling, request logging, rate limiting) must see the
// real client IP and scheme rather than the reverse proxy's.
app.UseForwardedHeaders();

app.UseCustomExceptionHandler();

if (!app.Environment.IsDevelopment()) app.UseHsts();

// HTTPS redirection intentionally disabled in dev compose (reverse proxy/TLS termination can be added in front)
app.UseResponseCompression();

app.UseSerilogRequestLogging();
app.UseRequestContextLogging();

app.UseRouting();
app.UseCors("DefaultCorsPolicy");

app.UseAuthentication();
app.UseAuthorization();

// After authentication so the "api" limiter can partition by user.
app.UseRateLimiter();

if (app.Environment.IsDevelopment())
{
    app.MapScalarApiReference();
    app.MapOpenApi();
}

app.MapControllers();

// Liveness: the process is up. Deliberately checks no dependency, so a Keycloak/Redis/Postgres blip never makes an
// orchestrator restart a healthy API.
app.MapHealthChecks("health/live", new HealthCheckOptions
{
    Predicate = _ => false
});

// Readiness: dependencies (Postgres, Redis, Keycloak) are reachable; use to gate traffic, not to restart.
app.MapHealthChecks("health/ready", new HealthCheckOptions
{
    Predicate = check => check.Tags.Contains("ready"),
    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
});

// Kept for backward compatibility (docker-compose healthcheck): all checks.
app.MapHealthChecks("health", new HealthCheckOptions
{
    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
});

await app.RunAsync();

public partial class Program
{
    // Prevent external instantiation while keeping class non-static for WebApplicationFactory
    protected Program() { }
}