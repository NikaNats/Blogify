namespace Blogify.Application.Abstractions.Authentication;

/// <summary>
///     The identity provider reports that a user with this username/email already exists. Retrying cannot succeed,
///     so callers must treat this as a permanent failure rather than a transient one.
/// </summary>
public sealed class IdentityAlreadyExistsException(string message) : Exception(message);
