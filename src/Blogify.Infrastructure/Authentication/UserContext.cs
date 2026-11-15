using System.Security.Claims;
using Blogify.Application.Abstractions.Authentication;
using Microsoft.AspNetCore.Http;

namespace Blogify.Infrastructure.Authentication;

internal sealed class UserContext(IHttpContextAccessor httpContextAccessor) : IUserContext
{
    private ClaimsPrincipal? Principal => httpContextAccessor.HttpContext?.User;

    public Guid? UserId
    {
        get
        {
            var principal = Principal;
            if (principal?.Identity?.IsAuthenticated is not true) return null;

            var id = principal.GetUserId();
            return id == Guid.Empty ? null : id;
        }
    }

    public string IdentityId => Principal?.GetIdentityId() ?? string.Empty;

    public bool IsInRole(string role) => Principal?.IsInRole(role) is true;
}
