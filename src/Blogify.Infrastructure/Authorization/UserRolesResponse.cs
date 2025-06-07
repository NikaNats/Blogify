using Blogify.Domain.Users;

namespace Blogify.Infrastructure.Authorization;

internal sealed class UserRolesResponse
{
    public Guid UserId { get; init; }

    public List<Role> Roles { get; init; } = [];
}