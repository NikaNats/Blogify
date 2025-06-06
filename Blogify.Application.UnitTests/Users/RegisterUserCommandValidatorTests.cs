using Blogify.Application.Users.RegisterUser;
using FluentValidation.Results;
using Shouldly;

namespace Blogify.Application.UnitTests.Users
{
    public class RegisterUserCommandValidatorTests
    {
        [Fact]
        public void Validate_ValidCommand_Succeeds()
        {
            var command = new RegisterUserCommand("test@example.com", "John", "Doe", "SecurePass123");
            var validator = new RegisterUserCommandValidator();

            ValidationResult result = validator.Validate(command);

            result.IsValid.ShouldBeTrue();
            result.Errors.ShouldBeEmpty();
        }

        [Fact]
        public void Validate_EmptyFirstName_FailsWithFirstNameError()
        {
            var command = new RegisterUserCommand("test@example.com", "", "Doe", "SecurePass123");
            var validator = new RegisterUserCommandValidator();

            ValidationResult result = validator.Validate(command);

            result.IsValid.ShouldBeFalse();
            result.Errors.ShouldHaveSingleItem().PropertyName.ShouldBe("FirstName");
        }

        [Fact]
        public void Validate_NullFirstName_FailsWithFirstNameError()
        {
            var command = new RegisterUserCommand("test@example.com", null!, "Doe", "SecurePass123");
            var validator = new RegisterUserCommandValidator();

            ValidationResult result = validator.Validate(command);

            result.IsValid.ShouldBeFalse();
            result.Errors.ShouldHaveSingleItem().PropertyName.ShouldBe("FirstName");
        }

        [Fact]
        public void Validate_EmptyLastName_FailsWithLastNameError()
        {
            var command = new RegisterUserCommand("test@example.com", "John", "", "SecurePass123");
            var validator = new RegisterUserCommandValidator();

            ValidationResult result = validator.Validate(command);

            result.IsValid.ShouldBeFalse();
            result.Errors.ShouldHaveSingleItem().PropertyName.ShouldBe("LastName");
        }

        [Fact]
        public void Validate_NullLastName_FailsWithLastNameError()
        {
            var command = new RegisterUserCommand("test@example.com", "John", null!, "SecurePass123");
            var validator = new RegisterUserCommandValidator();

            ValidationResult result = validator.Validate(command);

            result.IsValid.ShouldBeFalse();
            result.Errors.ShouldHaveSingleItem().PropertyName.ShouldBe("LastName");
        }

        [Fact]
        public void Validate_InvalidEmail_FailsWithEmailError()
        {
            var command = new RegisterUserCommand("invalid", "John", "Doe", "SecurePass123");
            var validator = new RegisterUserCommandValidator();

            ValidationResult result = validator.Validate(command);

            result.IsValid.ShouldBeFalse();
            result.Errors.ShouldContain(e => e.PropertyName == "Email");
        }

        [Fact]
        public void Validate_EmptyEmail_FailsWithEmailError()
        {
            var command = new RegisterUserCommand("", "John", "Doe", "SecurePass123");
            var validator = new RegisterUserCommandValidator();

            ValidationResult result = validator.Validate(command);

            result.IsValid.ShouldBeFalse();
            result.Errors.ShouldContain(e => e.PropertyName == "Email");
        }

        [Fact]
        public void Validate_NullEmail_FailsWithEmailError()
        {
            var command = new RegisterUserCommand(null!, "John", "Doe", "SecurePass123");
            var validator = new RegisterUserCommandValidator();

            ValidationResult result = validator.Validate(command);

            result.IsValid.ShouldBeFalse();
            result.Errors.ShouldContain(e => e.PropertyName == "Email");
        }

        [Fact]
        public void Validate_EmptyPassword_FailsWithPasswordErrors()
        {
            var command = new RegisterUserCommand("test@example.com", "John", "Doe", "");
            var validator = new RegisterUserCommandValidator();

            ValidationResult result = validator.Validate(command);

            result.IsValid.ShouldBeFalse();
            result.Errors.ShouldContain(e => e.PropertyName == "Password");
            result.Errors.Count(e => e.PropertyName == "Password").ShouldBe(2);
        }

        [Fact]
        public void Validate_NullPassword_FailsWithPasswordErrors()
        {
            var command = new RegisterUserCommand("test@example.com", "John", "Doe", null!);
            var validator = new RegisterUserCommandValidator();

            ValidationResult result = validator.Validate(command);

            result.IsValid.ShouldBeFalse();
            result.Errors.ShouldContain(e => e.PropertyName == "Password");
            result.Errors.Count(e => e.PropertyName == "Password").ShouldBe(1);
        }

        [Fact]
        public void Validate_ShortPassword_FailsWithPasswordError()
        {
            var command = new RegisterUserCommand("test@example.com", "John", "Doe", "1234");
            var validator = new RegisterUserCommandValidator();

            ValidationResult result = validator.Validate(command);

            result.IsValid.ShouldBeFalse();
            result.Errors.ShouldHaveSingleItem().PropertyName.ShouldBe("Password");
        }

        [Fact]
        public void Validate_MultipleInvalidProperties_FailsWithMultipleErrors()
        {
            var command = new RegisterUserCommand("", "", "", "");
            var validator = new RegisterUserCommandValidator();

            ValidationResult result = validator.Validate(command);

            result.IsValid.ShouldBeFalse();
            result.Errors.Count.ShouldBeGreaterThan(1);

            var propertyNames = result.Errors
                .Select(e => e.PropertyName)
                .Distinct()
                .ToList();

            propertyNames.ShouldContain("Email", "because the Email field is required");
            propertyNames.ShouldContain("FirstName", "because the FirstName field is required");
            propertyNames.ShouldContain("LastName", "because the LastName field is required");
            propertyNames.ShouldContain("Password", "because the Password field is required");

        }

    }
}
