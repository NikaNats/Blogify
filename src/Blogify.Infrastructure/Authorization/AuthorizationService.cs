using Blogify.Application.Abstractions.Caching;
using Blogify.Domain.Users;
using Microsoft.EntityFrameworkCore;

namespace Blogify.Infrastructure.Authorization;

internal sealed class AuthorizationService(ApplicationDbContext dbContext, ICacheService cacheService)
{
    public async Task<UserRolesResponse?> GetRolesForUserAsync(string identityId) // Return nullable
    {
        var cacheKey = $"auth:roles-{identityId}";
        var cachedRoles = await cacheService.GetAsync<UserRolesResponse>(cacheKey);

        if (cachedRoles is not null) return cachedRoles;

        // --- FIX: Use FirstOrDefaultAsync and handle the null case ---
        var roles = await dbContext.Set<User>()
            .Where(u => u.IdentityId == identityId)
            .Select(u => new UserRolesResponse
            {
                UserId = u.Id,
                Roles = u.Roles.ToList()
            })
            .FirstOrDefaultAsync(); // Changed from FirstAsync()

        if (roles is not null) await cacheService.SetAsync(cacheKey, roles);

        return roles; // Return the potentially null response
    }

    public async Task<HashSet<string>> GetPermissionsForUserAsync(string identityId)
    {
        var cacheKey = $"auth:permissions-{identityId}";
        var cachedPermissions = await cacheService.GetAsync<HashSet<string>>(cacheKey);

        if (cachedPermissions is not null) return cachedPermissions;

        // Make resilient: if user record is missing locally, return empty set (no permissions)
        var permissionNames = await dbContext.Set<User>()
            .Where(u => u.IdentityId == identityId)
            .Select(u => u.Roles
                .SelectMany(r => r.Permissions)
                .Select(p => p.Name)
                .ToHashSet())
            .FirstOrDefaultAsync();

        permissionNames ??= new HashSet<string>();

        await cacheService.SetAsync(cacheKey, permissionNames);

        return permissionNames;
    }
}