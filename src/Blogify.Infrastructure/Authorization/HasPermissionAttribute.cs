using Microsoft.AspNetCore.Authorization;

namespace Blogify.Infrastructure.Authorization;

public sealed class HasPermissionAttribute(string permission) : AuthorizeAttribute(permission);