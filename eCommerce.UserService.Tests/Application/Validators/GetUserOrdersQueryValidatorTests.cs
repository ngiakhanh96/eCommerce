using eCommerce.UserService.Application.Queries;
using eCommerce.UserService.Application.Validators;
using FluentValidation.TestHelper;

namespace eCommerce.UserService.Tests.Application.Validators;

/// <summary>
/// Unit tests for GetUserOrdersQueryValidator.
/// </summary>
public class GetUserOrdersQueryValidatorTests
{
    private readonly GetUserOrdersQueryValidator _validator;

    public GetUserOrdersQueryValidatorTests()
    {
        _validator = new GetUserOrdersQueryValidator();
    }

    [Fact]
    public void Validate_WithValidUserId_ShouldNotHaveErrors()
    {
        // Arrange
        var query = new GetUserOrdersQuery(Guid.NewGuid());

        // Act
        var result = _validator.TestValidate(query);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_WithEmptyUserId_ShouldHaveError()
    {
        // Arrange
        var query = new GetUserOrdersQuery(Guid.Empty);

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
        var query = new GetUserOrdersQuery(default);

        // Act
        var result = _validator.TestValidate(query);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.UserId)
            .WithErrorMessage("UserId is required.");
    }
}
