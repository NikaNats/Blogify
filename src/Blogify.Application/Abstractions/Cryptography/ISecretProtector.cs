namespace Blogify.Application.Abstractions.Cryptography;

/// <summary>
/// Protects short-lived secrets (e.g. a pending registration password) that must be
/// persisted temporarily, such as in the outbox, and recovered later by a background handler.
/// </summary>
public interface ISecretProtector
{
    string Protect(string plaintext);

    string Unprotect(string protectedValue);
}
