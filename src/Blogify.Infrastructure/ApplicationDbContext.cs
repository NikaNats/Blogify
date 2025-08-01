using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
using Blogify.Application.Abstractions.Clock;
using Blogify.Application.Exceptions;
using Blogify.Domain.Abstractions;
using Blogify.Infrastructure.Outbox;
using Microsoft.EntityFrameworkCore;

namespace Blogify.Infrastructure;

public sealed class ApplicationDbContext(
    DbContextOptions options,
    IDateTimeProvider dateTimeProvider)
    : DbContext(options), IUnitOfWork
{
    private static readonly JsonSerializerOptions DomainEventSerializerOptions = new(JsonSerializerDefaults.Web)
    {
        // JsonDerivedType attributes on IDomainEvent enable safe polymorphism.
        TypeInfoResolver = new DefaultJsonTypeInfoResolver(),
        WriteIndented = false
    };

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            AddDomainEventsAsOutboxMessages();

            var result = await base.SaveChangesAsync(cancellationToken);

            return result;
        }
        catch (DbUpdateConcurrencyException ex)
        {
            throw new ConcurrencyException("Concurrency exception occurred.", ex);
        }
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);

        base.OnModelCreating(modelBuilder);
    }

    private void AddDomainEventsAsOutboxMessages()
    {
        var outboxMessages = ChangeTracker
            .Entries<Entity>()
            .Select(entry => entry.Entity)
            .SelectMany(entity =>
            {
                var domainEvents = entity.DomainEvents;

                entity.ClearDomainEvents();

                return domainEvents;
            })
            .Select(domainEvent => new OutboxMessage(
                Guid.NewGuid(),
                dateTimeProvider.UtcNow,
                domainEvent.GetType().Name,
                JsonSerializer.Serialize(domainEvent, typeof(IDomainEvent), DomainEventSerializerOptions)))
            .ToList();

        AddRange(outboxMessages);
    }
}