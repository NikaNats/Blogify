namespace Blogify.Application.Abstractions.Authentication;

public interface IUserContext
{
    /// <summary>The local user id, or null when the caller is anonymous or unknown.</summary>
    Guid? UserId { get; }

    string IdentityId { get; }

    bool IsInRole(string role);
}
