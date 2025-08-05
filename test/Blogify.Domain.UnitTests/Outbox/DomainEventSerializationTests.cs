using System.Text.Json;
using Blogify.Domain.Abstractions;
using Blogify.Domain.Posts.Events;
using Shouldly;
using Xunit;

namespace Blogify.Domain.UnitTests.Outbox;

public class DomainEventSerializationTests
{
    // Local options: domain tests should not depend on Infrastructure layer
    private readonly JsonSerializerOptions _options = new(JsonSerializerDefaults.Web);

    [Fact]
    public void Serialize_And_Deserialize_Derived_Event_Preserves_Type()
    {
    var evt = new PostCreatedDomainEvent(Guid.NewGuid(), "Title", Guid.NewGuid());

        var json = JsonSerializer.Serialize<IDomainEvent>(evt, _options);

        json.ShouldContain("PostCreated"); // discriminator emitted
        json.ShouldContain("Title");

        var roundTripped = JsonSerializer.Deserialize<IDomainEvent>(json, _options);

        roundTripped.ShouldNotBeNull();
        roundTripped.ShouldBeOfType<PostCreatedDomainEvent>();
    ((PostCreatedDomainEvent)roundTripped).PostTitle.ShouldBe(evt.PostTitle);
    }

    [Fact]
    public void Deserialize_Unknown_Type_Fails()
    {
        var malicious = "{\"$type\":\"HackedEvent\",\"EventId\":\"" + Guid.NewGuid() + "\",\"OccurredOn\":\"2025-08-10T00:00:00Z\"}";

        Should.Throw<System.Text.Json.JsonException>(() =>
            JsonSerializer.Deserialize<IDomainEvent>(malicious, _options));
    }
}
