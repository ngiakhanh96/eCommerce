using eCommerce.UserService.Application.Queries;
using eCommerce.UserService.Application.Validators;
using FluentValidation.TestHelper;

namespace eCommerce.UserService.Tests.Application.Validators;

/// <summary>
/// Unit tests for GetUserQueryValidator.
/// </summary>
public class GetUserQueryValidatorTests
{
    private readonly GetUserQueryValidator _validator;

    public GetUserQueryValidatorTests()
    {
        _validator = new GetUserQueryValidator();
    }

    [Fact]
    public void Validate_WithValidUserId_ShouldNotHaveErrors()
    {
        // Arrange
        var query = new GetUserQuery(Guid.NewGuid());

        // Act
        var result = _validator.TestValidate(query);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_WithEmptyUserId_ShouldHaveError()
    {
        // Arrange
        var query = new GetUserQuery(Guid.Empty);

        // Act
        var result = _validator.TestValidate(query);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.UserId)
            .WithErrorMessage("UserId is required.");
    }

    [Fact]
    public void Validate_WithDefaultGuid_ShouldHaveError()
    {
        // Arrange
        var query = new GetUserQuery(default);

        // Act
        var result = _validator.TestValidate(query);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.UserId)
            .WithErrorMessage("UserId is required.");
    }
}
