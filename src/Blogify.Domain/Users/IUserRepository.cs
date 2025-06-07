using Blogify.Domain.Abstractions;

namespace Blogify.Domain.Users;

public interface IUserRepository : IRepository<User>
{
    Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);
    // Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    // Task AddAsync(User user, CancellationToken cancellationToken = default);
}