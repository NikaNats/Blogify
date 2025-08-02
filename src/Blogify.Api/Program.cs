using System.IO.Compression;
using Asp.Versioning.ApiExplorer;
using Blogify.Api.Controllers;
using Blogify.Api.Extensions;
using Blogify.Api.OpenApi;
using Blogify.Application;
using Blogify.Infrastructure;
using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.OpenApi.Models;
using Scalar.AspNetCore;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.AddOpenTelemetryMonitoring("Blogify.Api", ApiVersions.V1.ToString());

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

builder.Services.AddControllers();

builder.Services.AddCors(options =>
{
    options.AddPolicy("DefaultCorsPolicy", policy =>
    {
        policy.AllowAnyOrigin()
            .AllowAnyHeader()
            .AllowAnyMethod();
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

builder.Services.AddSwaggerGen();
builder.Services.ConfigureOptions<ConfigureSwaggerOptions>();

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
                Log.Error(ex, "Failed to apply database migrations after {Attempts} attempts", attempt);
                break; // Fail silently; app may still start (or you can rethrow)
            }
            var delay = TimeSpan.FromMilliseconds(baseDelay.TotalMilliseconds * Math.Pow(2, attempt - 1));
            Log.Warning(ex, "Migration attempt {Attempt} failed. Retrying in {Delay}...", attempt, delay);
            await Task.Delay(delay);
        }
    }
}

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

if (app.Environment.IsDevelopment())
{
    app.MapScalarApiReference();
    app.MapOpenApi();

    var provider = app.Services.GetRequiredService<IApiVersionDescriptionProvider>();
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        provider.ApiVersionDescriptions
            .Select(d => d.GroupName)
            .ToList()
            .ForEach(g => options.SwaggerEndpoint($"/swagger/{g}/swagger.json", g.ToUpperInvariant()));
    });
}

app.MapControllers();

app.MapHealthChecks("health", new HealthCheckOptions
{
    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
});

await app.RunAsync();

public partial class Program { }