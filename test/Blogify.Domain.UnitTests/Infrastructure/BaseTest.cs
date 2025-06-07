using Blogify.Domain.Abstractions;

namespace Blogify.Domain.UnitTests.Infrastructure;

public abstract class BaseTest
{
    public static T AssertDomainEventWasPublished<T>(Entity entity)
        where T : IDomainEvent
    {
        var domainEvent = entity.DomainEvents.OfType<T>().FirstOrDefault();

        if (domainEvent == null) throw new Exception($"{typeof(T).Name} was not published");

        return domainEvent;
    }
}