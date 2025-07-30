using Blogify.Application.Abstractions.Authentication;
using Microsoft.AspNetCore.Http;

namespace Blogify.Infrastructure.Authentication;

internal sealed class UserContext(IHttpContextAccessor httpContextAccessor) : IUserContext
{
    public Guid UserId =>
        httpContextAccessor
            .HttpContext?
            .User
            .GetUserId() ?? Guid.Empty; // Return Guid.Empty if unavailable; handlers can interpret as anonymous

    public string IdentityId =>
        httpContextAccessor
            .HttpContext?
            .User
            .GetIdentityId() ?? string.Empty; // Empty string signals absence without throwing
}