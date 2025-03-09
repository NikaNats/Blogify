using Blogify.Domain.Users;

namespace Blogify.Application.Abstractions.Authentication;

public interface IAuthenticationService
{
    Task<string> RegisterAsync(
        User user,
        string password,
        CancellationToken cancellationToken = default);
    
    Task DeleteIdentityAsync(
        string identityId,
        CancellationToken cancellationToken = default);
}