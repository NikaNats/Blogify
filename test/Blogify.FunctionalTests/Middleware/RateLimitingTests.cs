using System.Net;
using System.Net.Http.Json;
using Blogify.Api.RateLimiting;
using Blogify.FunctionalTests.Infrastructure;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;

namespace Blogify.FunctionalTests.Middleware;

public class RateLimitingTests(FunctionalTestWebAppFactory factory) : BaseFunctionalTest(factory)
{
    [Fact]
    public async Task Login_ExceedingAuthLimit_Returns429WithRetryAfterAndProblemDetails()
    {
        await using var limited = factory.WithWebHostBuilder(builder =>
            builder.ConfigureTestServices(services =>
                services.Configure<RateLimitingOptions>(o =>
                {
                    o.Enabled = true;
                    o.AuthPermitLimit = 2;
                    o.AuthWindowSeconds = 60;
                })));
        using var client = limited.CreateClient();

        var body = new { email = "nobody@example.com", password = "wrong-password" };
        HttpResponseMessage? throttled = null;
        for (var i = 0; i < 6 && throttled is null; i++)
        {
            var response = await client.PostAsJsonAsync("api/v1/users/login", body);
            if (response.StatusCode == HttpStatusCode.TooManyRequests) throttled = response;
        }

        throttled.ShouldNotBeNull();
        throttled.Headers.RetryAfter.ShouldNotBeNull();
        throttled.Content.Headers.ContentType!.MediaType.ShouldContain("json");
        (await throttled.Content.ReadAsStringAsync()).ShouldContain("429");
    }

    [Fact]
    public async Task HealthEndpoints_AreExemptFromRateLimiting()
    {
        await using var limited = factory.WithWebHostBuilder(builder =>
            builder.ConfigureTestServices(services =>
                services.Configure<RateLimitingOptions>(o =>
                {
                    o.Enabled = true;
                    o.ApiPermitLimit = 1;
                })));
        using var client = limited.CreateClient();

        for (var i = 0; i < 5; i++)
            (await client.GetAsync("health/live")).StatusCode.ShouldBe(HttpStatusCode.OK);
    }
}
