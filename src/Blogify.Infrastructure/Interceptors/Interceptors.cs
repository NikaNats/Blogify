using Blogify.Application.Abstractions.Authentication;
using Blogify.Domain.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.DependencyInjection;

namespace Blogify.Infrastructure.Interceptors;

public sealed class AuditableEntitySaveChangesInterceptor(IServiceProvider serviceProvider) : SaveChangesInterceptor
{
    public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
    {
        UpdateEntities(eventData.Context);
        return base.SavingChanges(eventData, result);
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        UpdateEntities(eventData.Context);
        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    private void UpdateEntities(DbContext? context)
    {
        if (context is null) return;

        var now = DateTimeOffset.UtcNow;
        var userId = GetCurrentUserId();

        foreach (var entry in context.ChangeTracker.Entries<AuditableEntity>())
        {
            if (entry.State == EntityState.Added)
            {
                // Use the EntityEntry.Property API to set values
                entry.Property(nameof(AuditableEntity.CreatedBy)).CurrentValue = userId;
                entry.Property(nameof(AuditableEntity.CreatedAt)).CurrentValue = now;
            }

            if (entry.State is EntityState.Added or EntityState.Modified)
            {
                // Use the EntityEntry.Property API to set values
                entry.Property(nameof(AuditableEntity.LastModifiedBy)).CurrentValue = userId;
                entry.Property(nameof(AuditableEntity.LastModifiedAt)).CurrentValue = now;
            }
        }
    }

    private Guid? GetCurrentUserId()
    {
        var userContext = serviceProvider.GetService<IUserContext>();
        try
        {
            return userContext?.UserId;
        }
        catch
        {
            return null;
        }
    }
}