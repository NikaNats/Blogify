using System.Text.Json.Serialization;

namespace Blogify.Infrastructure.Authentication.Models;

internal sealed class AuthorizationToken
{
    [JsonPropertyName("access_token")] public string AccessToken { get; init; } = string.Empty;

    /// <summary>Lifetime of the token in seconds, as reported by Keycloak.</summary>
    [JsonPropertyName("expires_in")] public int ExpiresIn { get; init; }
}
