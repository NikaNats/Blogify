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

app.UseCustomExceptionHandler();

if (!app.Environment.IsDevelopment()) app.UseHsts();

//app.UseHttpsRedirection();
app.UseResponseCompression();

app.UseSerilogRequestLogging();
app.UseRequestContextLogging();

app.UseRouting();
app.UseCors("DefaultCorsPolicy");

app.UseAuthentication();
app.UseAuthorization();

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