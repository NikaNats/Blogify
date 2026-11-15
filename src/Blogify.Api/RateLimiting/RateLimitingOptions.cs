namespace Blogify.Api.RateLimiting;

/// <summary>Bound from the <c>RateLimiting</c> configuration section. Defaults are the production values.</summary>
public sealed class RateLimitingOptions
{
    public const string SectionName = "RateLimiting";

    /// <summary>When false every limiter is a no-op (e.g. for local experiments); keep true in production.</summary>
    public bool Enabled { get; set; } = true;

    /// <summary>Requests per window, per client IP, for login and registration.</summary>
    public int AuthPermitLimit { get; set; } = 10;

    public int AuthWindowSeconds { get; set; } = 60;

    /// <summary>Requests per window for everything else, per user (authenticated) or client IP (anonymous).</summary>
    public int ApiPermitLimit { get; set; } = 100;

    public int ApiWindowSeconds { get; set; } = 60;
}
