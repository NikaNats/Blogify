using Blogify.Domain.Users;

namespace Blogify.Infrastructure.Repositories;

internal sealed class UserRepository : Repository<User>, IUserRepository
{
    public UserRepository(ApplicationDbContext dbContext)
        : base(dbContext)
    {
    }

    public async Task AddAsync(User user, CancellationToken cancellationToken = default)
    {
        foreach (var role in user.Roles) DbContext.Attach(role);

        await DbContext.AddAsync(user, cancellationToken);
    }

    public override void Add(User user)
    {
        foreach (var role in user.Roles) DbContext.Attach(role);

        DbContext.Add(user);
    }
}