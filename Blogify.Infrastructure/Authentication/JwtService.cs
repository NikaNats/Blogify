using System.Net.Http.Json;
using Blogify.Application.Abstractions.Authentication;
using Blogify.Domain.Abstractions;
using Blogify.Infrastructure.Authentication.Models;
using Microsoft.Extensions.Options;

namespace Blogify.Infrastructure.Authentication;

internal sealed class JwtService(HttpClient httpClient, IOptions<KeycloakOptions> keycloakOptions)
    : IJwtService
{
    private static readonly Error AuthenticationFailed = new(
        "Keycloak.AuthenticationFailed",
        "Failed to acquire access token do to authentication failure", ErrorType.AuthenticationFailed);

    private readonly KeycloakOptions _keycloakOptions = keycloakOptions.Value;

    public async Task<Result<string>> GetAccessTokenAsync(
        string email,
        string password,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var authRequestParameters = new KeyValuePair<string, string>[]
            {
                new("client_id", _keycloakOptions.AuthClientId),
                new("client_secret", _keycloakOptions.AuthClientSecret),
                new("scope", "openid email"),
                new("grant_type", "password"),
                new("username", email),
                new("password", password)
            };

            using var authorizationRequestContent = new FormUrlEncodedContent(authRequestParameters);

            var response = await httpClient.PostAsync(
                "",
                authorizationRequestContent,
                cancellationToken);
            
            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
                // Log error content here
                return Result.Failure<string>(AuthenticationFailed);
            }
            
            var authorizationToken = await response
                .Content
                .ReadFromJsonAsync<AuthorizationToken>(cancellationToken);

            if (authorizationToken is null) return Result.Failure<string>(AuthenticationFailed);

            return authorizationToken.AccessToken;
        }
        catch (HttpRequestException)
        {
            return Result.Failure<string>(AuthenticationFailed);
        }
    }
}