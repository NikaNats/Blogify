using MediatR;

namespace Blogify.Domain.Abstractions;

public interface IDomainEvent : INotification
{
    Guid EventId { get; }
    DateTimeOffset OccurredOn { get; }
}