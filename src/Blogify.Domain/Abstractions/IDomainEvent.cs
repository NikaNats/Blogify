using System.Text.Json.Serialization;
using Blogify.Domain.Categories.Events;
using Blogify.Domain.Comments.Events;
using Blogify.Domain.Posts.Events;
using Blogify.Domain.Tags.Events;
using Blogify.Domain.Users.Events;
using MediatR;

namespace Blogify.Domain.Abstractions;

// Polymorphic domain event whitelist for safe deserialization from the outbox
[JsonDerivedType(typeof(CategoryCreatedDomainEvent), typeDiscriminator: "CategoryCreated")]
[JsonDerivedType(typeof(CategoryUpdatedDomainEvent), typeDiscriminator: "CategoryUpdated")]
[JsonDerivedType(typeof(CommentAddedDomainEvent), typeDiscriminator: "CommentAdded")]
[JsonDerivedType(typeof(CommentDeletedDomainEvent), typeDiscriminator: "CommentDeleted")]
[JsonDerivedType(typeof(PostArchivedDomainEvent), typeDiscriminator: "PostArchived")]
[JsonDerivedType(typeof(PostCategoryAddedDomainEvent), typeDiscriminator: "PostCategoryAdded")]
[JsonDerivedType(typeof(PostCategoryRemovedDomainEvent), typeDiscriminator: "PostCategoryRemoved")]
[JsonDerivedType(typeof(PostCreatedDomainEvent), typeDiscriminator: "PostCreated")]
[JsonDerivedType(typeof(PostPublishedDomainEvent), typeDiscriminator: "PostPublished")]
[JsonDerivedType(typeof(PostTaggedDomainEvent), typeDiscriminator: "PostTagged")]
[JsonDerivedType(typeof(PostUntaggedDomainEvent), typeDiscriminator: "PostUntagged")]
[JsonDerivedType(typeof(PostUpdatedDomainEvent), typeDiscriminator: "PostUpdated")]
[JsonDerivedType(typeof(TagCreatedDomainEvent), typeDiscriminator: "TagCreated")]
[JsonDerivedType(typeof(EmailChangedDomainEvent), typeDiscriminator: "EmailChanged")]
[JsonDerivedType(typeof(RoleAssignedDomainEvent), typeDiscriminator: "RoleAssigned")]
[JsonDerivedType(typeof(UserCreatedDomainEvent), typeDiscriminator: "UserCreated")]
[JsonDerivedType(typeof(UserNameChangedDomainEvent), typeDiscriminator: "UserNameChanged")]
public interface IDomainEvent : INotification
{
    Guid EventId { get; }
    DateTimeOffset OccurredOn { get; }
}