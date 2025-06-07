using System.Net.Mail;
using Blogify.Domain.Abstractions;

namespace Blogify.Domain.Users;

public sealed class Email : ValueObject
{
    private const int MaxLength = 254; // RFC 5321 SMTP limit

    private Email(string address)
    {
        Address = address;
    }

    public string Address { get; }

    public static Result<Email> Create(string emailAddress)
    {
        if (string.IsNullOrEmpty(emailAddress))
            return Result.Failure<Email>(UserErrors.InvalidEmail);

        if (emailAddress.Length > MaxLength)
            return Result.Failure<Email>(UserErrors.EmailTooLong);

        var trimmedEmail = emailAddress.Trim();

        try
        {
            var mailAddress = new MailAddress(trimmedEmail);
            var canonicalAddress = mailAddress.Address.ToLowerInvariant();
            return Result.Success(new Email(canonicalAddress));
        }
        catch (FormatException)
        {
            return Result.Failure<Email>(UserErrors.InvalidEmail);
        }
    }

    protected override IEnumerable<object> GetAtomicValues()
    {
        yield return Address;
    }

    public static implicit operator string(Email email)
    {
        return email.Address;
    }
}