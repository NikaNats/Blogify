using Blogify.Api.Middleware;
using Blogify.Infrastructure;
using Blogify.Application.Users.RegisterUser; // for RegisterUserCommand masking
using Blogify.Api.Controllers.Users; // for LogInUserRequest masking
using Microsoft.EntityFrameworkCore;
using Npgsql;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Serilog;

namespace Blogify.Api.Extensions;

internal static class ApplicationBuilderExtensions
{
    public static void ApplyMigrations(this IApplicationBuilder app)
    {
        using var scope = app.ApplicationServices.CreateScope();

        using var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        dbContext.Database.Migrate();
    }

    public static void UseCustomExceptionHandler(this IApplicationBuilder app)
    {
        app.UseMiddleware<ExceptionHandlingMiddleware>();
    }

    public static void UseRequestContextLogging(this IApplicationBuilder app)
    {
        app.UseMiddleware<RequestContextLoggingMiddleware>();
    }

    public static void AddOpenTelemetryMonitoring(this WebApplicationBuilder builder, string serviceName,
        string version)
    {
        var resourceBuilder = ResourceBuilder.CreateDefault()
            .AddService(serviceName, serviceVersion: version)
            .AddAttributes(new Dictionary<string, object>
            {
                ["service.instance.id"] = Environment.MachineName
            });

        builder.Host.UseSerilog((context, loggerConfig) =>
        {
            loggerConfig.ReadFrom.Configuration(context.Configuration);

            // Mask sensitive data (PII / credentials) at destructuring time
            loggerConfig
                .Destructure.ByTransforming<RegisterUserCommand>(c => new
                {
                    c.Email,
                    c.FirstName,
                    c.LastName,
                    Password = "*** MASKED ***"
                })
                .Destructure.ByTransforming<LogInUserRequest>(r => new
                {
                    r.Email,
                    Password = "*** MASKED ***"
                });

            loggerConfig.WriteTo.OpenTelemetry(options =>
            {
                options.ResourceAttributes = resourceBuilder.Build().Attributes
                    .ToDictionary(k => k.Key, v => v.Value);
            });
        });

        builder.Services.AddOpenTelemetry()
            .WithTracing(tracing =>
            {
                tracing
                    .SetResourceBuilder(resourceBuilder)
                    .AddHttpClientInstrumentation()
                    .AddAspNetCoreInstrumentation(options =>
                    {
                        // Capture exceptions as span events for better diagnostics
                        options.RecordException = true;
                    })
                    .AddNpgsql();

                // Environment-based adaptive sampling:
                // - Development: AlwaysOn for full fidelity debugging.
                // - Non-development: 10% head-based, parent-based to preserve existing traces.
                if (!builder.Environment.IsDevelopment())
                {
                    tracing.SetSampler(new ParentBasedSampler(new TraceIdRatioBasedSampler(0.1)));
                }
                else
                {
                    tracing.SetSampler(new AlwaysOnSampler());
                }

                tracing.AddOtlpExporter();
            })
            .WithMetrics(metrics =>
            {
                metrics
                    .SetResourceBuilder(resourceBuilder)
                    .AddHttpClientInstrumentation()
                    .AddAspNetCoreInstrumentation()
                    .AddRuntimeInstrumentation()
                    .AddOtlpExporter();
            });
    }
}