using Asp.Versioning.ApiExplorer;
using Blogify.Api.Extensions;
using Blogify.Api.OpenApi;
using Blogify.Application;
using Blogify.Infrastructure;
using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.OpenApi.Models;
using Scalar.AspNetCore;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((context, loggerConfig) =>
    loggerConfig.ReadFrom.Configuration(context.Configuration));

builder.Services.AddControllers();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

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

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    try
    {
        app.ApplyMigrations();
    }
    catch (Exception ex)
    {
        Log.Error(ex, "An error occurred while applying migrations.");
    }

    app.MapScalarApiReference();

    app.MapOpenApi();

    var provider = app.Services.GetRequiredService<IApiVersionDescriptionProvider>();
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        foreach (var description in provider.ApiVersionDescriptions)
            options.SwaggerEndpoint(
                $"/swagger/{description.GroupName}/swagger.json",
                description.GroupName.ToUpperInvariant());
    });

    // REMARK: Uncomment if you want to seed initial data.
    // await app.SeedDataAsync();
}

if (!app.Environment.IsDevelopment()) app.UseHsts();

app.UseCustomExceptionHandler();

app.UseSerilogRequestLogging();

app.UseRequestContextLogging();

app.UseCors("AllowAll");

// app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.MapHealthChecks("health", new HealthCheckOptions
{
    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
});

app.Run();

namespace Blogify.Api
{
    public class Program;
}