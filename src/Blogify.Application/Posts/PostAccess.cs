using Blogify.Application.Abstractions.Authentication;
using Blogify.Domain.Posts;

namespace Blogify.Application.Posts;

internal static class PostAccess
{
    public const string AdministratorRole = "Administrator";

    /// <summary>Published posts are public; Draft and Archived ones are visible to their author and administrators.</summary>
    public static bool CanView(PublicationStatus status, Guid authorId, IUserContext userContext)
    {
        return status == PublicationStatus.Published ||
               userContext.IsInRole(AdministratorRole) ||
               (userContext.UserId is { } userId && userId == authorId);
    }

    /// <summary>A post may be modified by its author or by an administrator.</summary>
    public static bool CanModify(Post post, IUserContext userContext)
    {
        return userContext.IsInRole(AdministratorRole) ||
               (userContext.UserId is { } userId && userId == post.AuthorId);
    }
}
