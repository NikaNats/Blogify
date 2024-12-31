using System.Net.Mail;
using Blogify.Domain.Abstractions;

namespace Blogify.Domain.Users
{ 
    public sealed class Email : ValueObject
    {
        public string Address { get; private set; }

        private Email(string address)
        {
            Address = address;
        }

        public static Result<Email> Create(string emailAddress)
        {
            if (string.IsNullOrEmpty(emailAddress))
                return Result.Failure<Email>(UserErrors.InvalidEmail);

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
    }
    
}