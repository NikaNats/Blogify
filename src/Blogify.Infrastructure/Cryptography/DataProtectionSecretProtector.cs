using Blogify.Application.Abstractions.Cryptography;
using Microsoft.AspNetCore.DataProtection;

namespace Blogify.Infrastructure.Cryptography;

internal sealed class DataProtectionSecretProtector(IDataProtectionProvider dataProtectionProvider) : ISecretProtector
{
    private const string Purpose = "Blogify.Users.PendingRegistration.Password.v1";

    private readonly IDataProtector _protector = dataProtectionProvider.CreateProtector(Purpose);

    public string Protect(string plaintext) => _protector.Protect(plaintext);

    public string Unprotect(string protectedValue) => _protector.Unprotect(protectedValue);
}
