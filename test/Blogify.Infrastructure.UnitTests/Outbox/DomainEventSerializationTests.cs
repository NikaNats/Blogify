using System.Text.Json;
using Blogify.Domain.Abstractions;
using Blogify.Domain.Posts.Events;
using Blogify.Infrastructure.Serialization;
using Shouldly;
using Xunit;

namespace Blogify.Infrastructure.UnitTests.Outbox;

public class DomainEventSerializationTests
{
    [Fact]
    public void PostCreated_RoundTrip_PreservesConcreteType()
    {
        // Arrange
        IDomainEvent original = new PostCreatedDomainEvent(Guid.NewGuid(), "Sample Title", Guid.NewGuid());

        // Act
        var json = JsonSerializer.Serialize(original, typeof(IDomainEvent), DomainEventSerializer.Options);
        var deserialized = JsonSerializer.Deserialize<IDomainEvent>(json, DomainEventSerializer.Options);

        // Assert
    var typed = deserialized.ShouldBeOfType<PostCreatedDomainEvent>();
    var originalTyped = (PostCreatedDomainEvent)original;

    typed.PostId.ShouldBe(originalTyped.PostId);
    typed.PostTitle.ShouldBe(originalTyped.PostTitle);
    typed.AuthorId.ShouldBe(originalTyped.AuthorId);
    }
}
