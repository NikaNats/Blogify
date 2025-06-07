namespace Blogify.Domain.Abstractions;

public interface IUnitOfWork : IDisposable, IAsyncDisposable
{
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}