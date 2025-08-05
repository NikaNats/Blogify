using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
using Blogify.Domain.Abstractions;

namespace Blogify.Infrastructure.Serialization;

/// <summary>
/// Provides a single, consistent configuration for serializing and deserializing domain events,
/// ensuring that polymorphic deserialization works correctly across the application.
/// </summary>
internal static class DomainEventSerializer
{
    /// <summary>
    /// Gets the globally shared JsonSerializerOptions for domain events.
    /// This instance is configured to handle the polymorphic nature of IDomainEvent using
    /// the [JsonDerivedType] attributes defined on the interface.
    /// </summary>
    public static readonly JsonSerializerOptions Options = new(JsonSerializerDefaults.Web)
    {
        // This TypeInfoResolver is crucial for recognizing the [JsonDerivedType] attributes on IDomainEvent,
        // which enables safe and correct polymorphic deserialization.
        TypeInfoResolver = new DefaultJsonTypeInfoResolver(),
        WriteIndented = false
    };
}
