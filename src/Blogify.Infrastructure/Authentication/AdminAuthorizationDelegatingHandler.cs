using System.Net.Http.Headers;
using System.Net.Http.Json;
using Blogify.Infrastructure.Authentication.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;

namespace Blogify.Infrastructure.Authentication;

/// <summary>
///     Adds a Keycloak admin (client-credentials) bearer token to outgoing requests. The token is cached until shortly
///     before it expires, so we do not make an extra token round trip for every admin API call.
/// </summary>
internal sealed class AdminAuthorizationDelegatingHandler(
    IOptions<KeycloakOptions> keycloakOptions,
    IMemoryCache cache) : DelegatingHandler
{
    private const string TokenCacheKey = "keycloak:admin-token";
    private static readonly TimeSpan ExpirySafetyMargin = TimeSpan.FromSeconds(30);
    private static readonly SemaphoreSlim RefreshLock = new(1, 1);

    private readonly KeycloakOptions _keycloakOptions = keycloakOptions.Value;

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        var accessToken = await GetAccessTokenAsync(cancellationToken);

        request.Headers.Authorization = new AuthenticationHeaderValue(
            JwtBearerDefaults.AuthenticationScheme,
            accessToken);

        var response = await base.SendAsync(request, cancellationToken);

        // The cached token may have been revoked server side; drop it so the next call fetches a fresh one.
        if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized) cache.Remove(TokenCacheKey);

        // Status handling is left to the caller (e.g. 409 on duplicate user is not a transient failure).
        return response;
    }

    private async Task<string> GetAccessTokenAsync(CancellationToken cancellationToken)
    {
        if (cache.TryGetValue(TokenCacheKey, out string? cached) && cached is not null) return cached;

        await RefreshLock.WaitAsync(cancellationToken);
        try
        {
            if (cache.TryGetValue(TokenCacheKey, out cached) && cached is not null) return cached;

            var token = await RequestTokenAsync(cancellationToken);

            var lifetime = TimeSpan.FromSeconds(token.ExpiresIn) - ExpirySafetyMargin;
            if (lifetime > TimeSpan.Zero) cache.Set(TokenCacheKey, token.AccessToken, lifetime);

            return token.AccessToken;
        }
        finally
        {
            RefreshLock.Release();
        }
    }

    private async Task<AuthorizationToken> RequestTokenAsync(CancellationToken cancellationToken)
    {
        var parameters = new KeyValuePair<string, string>[]
        {
            new("client_id", _keycloakOptions.AdminClientId),
            new("client_secret", _keycloakOptions.AdminClientSecret),
            new("scope", "openid email"),
            new("grant_type", "client_credentials")
        };

        using var content = new FormUrlEncodedContent(parameters);
        using var tokenRequest = new HttpRequestMessage(HttpMethod.Post, new Uri(_keycloakOptions.TokenUrl))
        {
            Content = content
        };

        using var tokenResponse = await base.SendAsync(tokenRequest, cancellationToken);
        tokenResponse.EnsureSuccessStatusCode();

        return await tokenResponse.Content.ReadFromJsonAsync<AuthorizationToken>(cancellationToken) ??
               throw new InvalidOperationException("Keycloak returned an empty token response.");
    }
}
