using System.Net;
using Microsoft.AspNetCore.HttpOverrides;

namespace Blogify.Api.Extensions;

internal static class ForwardedHeadersExtensions
{
    /// <summary>
    ///     Honors X-Forwarded-For / X-Forwarded-Proto from a reverse proxy so client IPs (rate limiting, logs) and the
    ///     request scheme are correct. Only proxies listed under <c>ForwardedHeaders:KnownProxies</c> (IPs) or
    ///     <c>ForwardedHeaders:KnownNetworks</c> (CIDR) are trusted; with none configured only loopback is, so a client
    ///     can never spoof its address by sending the header directly.
    /// </summary>
    public static IServiceCollection AddBlogifyForwardedHeaders(this IServiceCollection services,
        IConfiguration configuration)
    {
        var proxies = configuration.GetSection("ForwardedHeaders:KnownProxies").Get<string[]>() ?? [];
        var networks = configuration.GetSection("ForwardedHeaders:KnownNetworks").Get<string[]>() ?? [];

        services.Configure<ForwardedHeadersOptions>(options =>
        {
            options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;

            foreach (var proxy in proxies) options.KnownProxies.Add(IPAddress.Parse(proxy));

            foreach (var network in networks)
            {
                var parts = network.Split('/');
                options.KnownIPNetworks.Add(new System.Net.IPNetwork(IPAddress.Parse(parts[0]), int.Parse(parts[1])));
            }
        });

        return services;
    }
}
