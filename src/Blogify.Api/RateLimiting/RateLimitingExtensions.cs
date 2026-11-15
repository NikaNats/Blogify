using System.Globalization;
using System.Security.Claims;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Options;

namespace Blogify.Api.RateLimiting;

internal static class RateLimitingExtensions
{
    public const string AuthPolicy = "auth";

    public static IServiceCollection AddBlogifyRateLimiting(this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<RateLimitingOptions>(configuration.GetSection(RateLimitingOptions.SectionName));

        // Configured lazily from IOptions so tests can override the limits without touching Program.cs.
        services.AddOptions<RateLimiterOptions>()
            .Configure<IOptions<RateLimitingOptions>>((options, limits) => Configure(options, limits.Value));

        services.AddRateLimiter(_ => { });
        return services;
    }

    private static void Configure(RateLimiterOptions options, RateLimitingOptions limits)
    {
        options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
        options.OnRejected = WriteRejectionAsync;

        // Named policy for login/register: brute-force and spam protection, partitioned by client IP.
        options.AddPolicy(AuthPolicy, context => limits.Enabled
            ? RateLimitPartition.GetFixedWindowLimiter(
                "auth:" + ClientIp(context),
                _ => FixedWindow(limits.AuthPermitLimit, limits.AuthWindowSeconds))
            : RateLimitPartition.GetNoLimiter("auth"));

        // Global "api" limiter: per user when authenticated, otherwise per client IP. Probes are never limited.
        options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(context =>
        {
            if (!limits.Enabled || context.Request.Path.StartsWithSegments("/health"))
                return RateLimitPartition.GetNoLimiter("unlimited");

            return RateLimitPartition.GetFixedWindowLimiter(
                "api:" + ClientKey(context),
                _ => FixedWindow(limits.ApiPermitLimit, limits.ApiWindowSeconds));
        });
    }

    private static FixedWindowRateLimiterOptions FixedWindow(int permitLimit, int windowSeconds) => new()
    {
        PermitLimit = permitLimit,
        Window = TimeSpan.FromSeconds(windowSeconds),
        QueueLimit = 0,
        AutoReplenishment = true
    };

    private static string ClientIp(HttpContext context) =>
        context.Connection.RemoteIpAddress?.ToString() ?? "unknown";

    private static string ClientKey(HttpContext context)
    {
        if (context.User.Identity?.IsAuthenticated is true)
        {
            var userId = context.User.FindFirstValue(ClaimTypes.NameIdentifier) ?? context.User.FindFirstValue("sub");
            if (userId is not null) return "user:" + userId;
        }

        return "ip:" + ClientIp(context);
    }

    private static async ValueTask WriteRejectionAsync(OnRejectedContext context, CancellationToken cancellationToken)
    {
        var response = context.HttpContext.Response;
        response.StatusCode = StatusCodes.Status429TooManyRequests;

        var detail = "Too many requests. Please try again later.";
        if (context.Lease.TryGetMetadata(MetadataName.RetryAfter, out var retryAfter))
        {
            var seconds = Math.Max(1, (int)Math.Ceiling(retryAfter.TotalSeconds));
            response.Headers.RetryAfter = seconds.ToString(CultureInfo.InvariantCulture);
            detail = $"Too many requests. Retry after {seconds} seconds.";
        }

        // RFC 9457 problem details.
        await response.WriteAsJsonAsync(new ProblemDetails
        {
            Title = "TooManyRequests",
            Type = "https://api.blogify.com/errors/too-many-requests",
            Status = StatusCodes.Status429TooManyRequests,
            Detail = detail,
            Instance = context.HttpContext.Request.Path.Value
        }, cancellationToken);
    }
}
