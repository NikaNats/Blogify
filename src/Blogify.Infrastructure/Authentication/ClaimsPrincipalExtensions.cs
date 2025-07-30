using System.Security.Claims;
using Microsoft.IdentityModel.JsonWebTokens;

namespace Blogify.Infrastructure.Authentication;

internal static class ClaimsPrincipalExtensions
{
    public static Guid GetUserId(this ClaimsPrincipal? principal)
    {
        var userId = principal?.FindFirstValue(JwtRegisteredClaimNames.Sub);
    // If the claim is missing or not a valid Guid for a local user (e.g. external IdP subject),
    // return Guid.Empty instead of throwing to allow upstream code to handle gracefully.
    return Guid.TryParse(userId, out var parsedUserId) ? parsedUserId : Guid.Empty;
    }

    public static string GetIdentityId(this ClaimsPrincipal? principal)
    {
    // Return empty string instead of throwing; authorization layer will treat this as no local identity.
    return principal?.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
    }
}