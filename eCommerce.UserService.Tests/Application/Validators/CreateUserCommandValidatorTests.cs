using eCommerce.UserService.Application.Commands;
using eCommerce.UserService.Application.Validators;
using FluentValidation.TestHelper;

namespace eCommerce.UserService.Tests.Application.Validators;

/// <summary>
/// Unit tests for CreateUserCommandValidator.
/// </summary>
public class CreateUserCommandValidatorTests
{
    private readonly CreateUserCommandValidator _validator;

    public CreateUserCommandValidatorTests()
    {
        _validator = new CreateUserCommandValidator();
    }

    [Fact]
    public void Validate_WithValidCommand_ShouldNotHaveErrors()
    {
        // Arrange
        var command = new CreateUserCommand
        {
            Name = "John Doe",
            Email = "john@example.com"
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Validate_WithEmptyName_ShouldHaveError(string? name)
    {
        // Arrange
        var command = new CreateUserCommand
        {
            Name = name!,
            Email = "john@example.com"
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Name);
    }

    [Fact]
    public void Validate_WithNameExceeding255Characters_ShouldHaveError()
    {
        // Arrange
        var command = new CreateUserCommand
        {
            Name = new string('A', 256),
            Email = "john@example.com"
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Name)
            .WithErrorMessage("Name must not exceed 255 characters.");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Validate_WithEmptyEmail_ShouldHaveError(string? email)
    {
        // Arrange
        var command = new CreateUserCommand
        {
            Name = "John Doe",
            Email = email!
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Email);
    }

    [Theory]
    [InlineData("invalidemail")]
    [InlineData("invalid.email")]
    [InlineData("@invalid.com")]
    [InlineData("invalid@")]
    public void Validate_WithInvalidEmailFormat_ShouldHaveError(string email)
    {
        // Arrange
        var command = new CreateUserCommand
        {
            Name = "John Doe",
            Email = email
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Email)
            .WithErrorMessage("Email must be a valid email address.");
    }

    [Fact]
    public void Validate_WithEmailExceeding255Characters_ShouldHaveError()
    {
        // Arrange
        var command = new CreateUserCommand
        {
            Name = "John Doe",
            Email = new string('a', 250) + "@b.com"
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Email)
            .WithErrorMessage("Email must not exceed 255 characters.");
    }

    [Theory]
    [InlineData("user@example.com")]
    [InlineData("user.name@example.com")]
    [InlineData("user+tag@example.com")]
    [InlineData("user@subdomain.example.com")]
    public void Validate_WithValidEmailFormats_ShouldNotHaveError(string email)
    {
        // Arrange
        var command = new CreateUserCommand
        {
            Name = "John Doe",
            Email = email
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Email);
    }

    [Fact]
    public void Validate_WithMultipleErrors_ShouldReturnAllErrors()
    {
        // Arrange
        var command = new CreateUserCommand
        {
            Name = "",
            Email = "invalid"
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Name);
        result.ShouldHaveValidationErrorFor(x => x.Email);
    }
}
