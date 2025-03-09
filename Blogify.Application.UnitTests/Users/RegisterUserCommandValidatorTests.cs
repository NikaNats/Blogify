using Blogify.Application.Users.RegisterUser;
using FluentAssertions;
using FluentValidation.Results;

namespace Blogify.Application.UnitTests.Users
{
    public class RegisterUserCommandValidatorTests
    {
        /// <summary>
        /// Tests that a command with all valid properties passes validation.
        /// </summary>
        [Fact]
        public void Validate_ValidCommand_Succeeds()
        {
            // Arrange
            var command = new RegisterUserCommand(
                Email: "test@example.com",
                FirstName: "John",
                LastName: "Doe",
                Password: "SecurePass123"
            );
            var validator = new RegisterUserCommandValidator();

            // Act
            ValidationResult result = validator.Validate(command);

            // Assert
            result.IsValid.Should().BeTrue();
            result.Errors.Should().BeEmpty();
        }

        /// <summary>
        /// Tests that an empty FirstName fails validation with an error for FirstName.
        /// </summary>
        [Fact]
        public void Validate_EmptyFirstName_FailsWithFirstNameError()
        {
            // Arrange
            var command = new RegisterUserCommand(
                Email: "test@example.com",
                FirstName: "",
                LastName: "Doe",
                Password: "SecurePass123"
            );
            var validator = new RegisterUserCommandValidator();

            // Act
            ValidationResult result = validator.Validate(command);

            // Assert
            result.IsValid.Should().BeFalse();
            result.Errors.Should().ContainSingle()
                .Which.PropertyName.Should().Be("FirstName");
        }

        /// <summary>
        /// Tests that a null FirstName fails validation with an error for FirstName.
        /// </summary>
        [Fact]
        public void Validate_NullFirstName_FailsWithFirstNameError()
        {
            // Arrange
            var command = new RegisterUserCommand(
                Email: "test@example.com",
                FirstName: null!,
                LastName: "Doe",
                Password: "SecurePass123"
            );
            var validator = new RegisterUserCommandValidator();

            // Act
            ValidationResult result = validator.Validate(command);

            // Assert
            result.IsValid.Should().BeFalse();
            result.Errors.Should().ContainSingle()
                .Which.PropertyName.Should().Be("FirstName");
        }

        /// <summary>
        /// Tests that an empty LastName fails validation with an error for LastName.
        /// </summary>
        [Fact]
        public void Validate_EmptyLastName_FailsWithLastNameError()
        {
            // Arrange
            var command = new RegisterUserCommand(
                Email: "test@example.com",
                FirstName: "John",
                LastName: "",
                Password: "SecurePass123"
            );
            var validator = new RegisterUserCommandValidator();

            // Act
            ValidationResult result = validator.Validate(command);

            // Assert
            result.IsValid.Should().BeFalse();
            result.Errors.Should().ContainSingle()
                .Which.PropertyName.Should().Be("LastName");
        }

        /// <summary>
        /// Tests that a null LastName fails validation with an error for LastName.
        /// </summary>
        [Fact]
        public void Validate_NullLastName_FailsWithLastNameError()
        {
            // Arrange
            var command = new RegisterUserCommand(
                Email: "test@example.com",
                FirstName: "John",
                LastName: null!,
                Password: "SecurePass123"
            );
            var validator = new RegisterUserCommandValidator();

            // Act
            ValidationResult result = validator.Validate(command);

            // Assert
            result.IsValid.Should().BeFalse();
            result.Errors.Should().ContainSingle()
                .Which.PropertyName.Should().Be("LastName");
        }

        /// <summary>
        /// Tests that an invalid email fails validation with an error for Email.
        /// </summary>
        [Fact]
        public void Validate_InvalidEmail_FailsWithEmailError()
        {
            // Arrange
            var command = new RegisterUserCommand(
                Email: "invalid",
                FirstName: "John",
                LastName: "Doe",
                Password: "SecurePass123"
            );
            var validator = new RegisterUserCommandValidator();

            // Act
            ValidationResult result = validator.Validate(command);

            // Assert
            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(e => e.PropertyName == "Email");
        }

        /// <summary>
        /// Tests that an empty email fails validation with an error for Email.
        /// </summary>
        [Fact]
        public void Validate_EmptyEmail_FailsWithEmailError()
        {
            // Arrange
            var command = new RegisterUserCommand(
                Email: "",
                FirstName: "John",
                LastName: "Doe",
                Password: "SecurePass123"
            );
            var validator = new RegisterUserCommandValidator();

            // Act
            ValidationResult result = validator.Validate(command);

            // Assert
            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(e => e.PropertyName == "Email");
        }

        /// <summary>
        /// Tests that a null email fails validation with an error for Email.
        /// </summary>
        [Fact]
        public void Validate_NullEmail_FailsWithEmailError()
        {
            // Arrange
            var command = new RegisterUserCommand(
                Email: null!,
                FirstName: "John",
                LastName: "Doe",
                Password: "SecurePass123"
            );
            var validator = new RegisterUserCommandValidator();

            // Act
            ValidationResult result = validator.Validate(command);

            // Assert
            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(e => e.PropertyName == "Email");
        }

        /// <summary>
        /// Tests that an empty password fails validation with errors for Password.
        /// </summary>
        [Fact]
        public void Validate_EmptyPassword_FailsWithPasswordErrors()
        {
            // Arrange
            var command = new RegisterUserCommand(
                Email: "test@example.com",
                FirstName: "John",
                LastName: "Doe",
                Password: ""
            );
            var validator = new RegisterUserCommandValidator();

            // Act
            ValidationResult result = validator.Validate(command);

            // Assert
            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(e => e.PropertyName == "Password");
            result.Errors.Where(e => e.PropertyName == "Password").Should().HaveCount(2); // NotEmpty and MinimumLength
        }

        /// <summary>
        /// Tests that a null password fails validation with errors for Password.
        /// </summary>
        [Fact]
        public void Validate_NullPassword_FailsWithPasswordErrors()
        {
            // Arrange
            var command = new RegisterUserCommand(
                Email: "test@example.com",
                FirstName: "John",
                LastName: "Doe",
                Password: null!
            );
            var validator = new RegisterUserCommandValidator();

            // Act
            ValidationResult result = validator.Validate(command);

            // Assert
            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(e => e.PropertyName == "Password");
            result.Errors.Where(e => e.PropertyName == "Password").Should().ContainSingle(); // Only NotEmpty applies
        }

        /// <summary>
        /// Tests that a short password (less than 5 characters) fails validation with an error for Password.
        /// </summary>
        [Fact]
        public void Validate_ShortPassword_FailsWithPasswordError()
        {
            // Arrange
            var command = new RegisterUserCommand(
                Email: "test@example.com",
                FirstName: "John",
                LastName: "Doe",
                Password: "1234"
            );
            var validator = new RegisterUserCommandValidator();

            // Act
            ValidationResult result = validator.Validate(command);

            // Assert
            result.IsValid.Should().BeFalse();
            result.Errors.Should().ContainSingle()
                .Which.PropertyName.Should().Be("Password");
        }

        /// <summary>
        /// Tests that a command with multiple invalid properties fails validation with errors for each invalid property.
        /// </summary>
        [Fact]
        public void Validate_MultipleInvalidProperties_FailsWithMultipleErrors()
        {
            // Arrange
            var command = new RegisterUserCommand(
                Email: "",
                FirstName: "",
                LastName: "",
                Password: ""
            );
            var validator = new RegisterUserCommandValidator();

            // Act
            ValidationResult result = validator.Validate(command);

            // Assert
            result.IsValid.Should().BeFalse();
            result.Errors.Should().HaveCountGreaterThan(1);
            result.Errors.Select(e => e.PropertyName)
                .Should().Contain(new[] { "Email", "FirstName", "LastName", "Password" });
        }
    }
}