using eCommerce.OrderService.Application.Queries;
using eCommerce.OrderService.Application.Validators;
using FluentValidation.TestHelper;

namespace eCommerce.OrderService.Tests.Application.Validators;

/// <summary>
/// Unit tests for GetOrderQueryValidator.
/// </summary>
public class GetOrderQueryValidatorTests
{
    private readonly GetOrderQueryValidator _validator;

    public GetOrderQueryValidatorTests()
    {
        _validator = new GetOrderQueryValidator();
    }

    [Fact]
    public void Validate_WithValidOrderId_ShouldNotHaveErrors()
    {
        // Arrange
        var query = new GetOrderQuery(Guid.NewGuid());

        // Act
        var result = _validator.TestValidate(query);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_WithEmptyOrderId_ShouldHaveError()
    {
        // Arrange
        var query = new GetOrderQuery(Guid.Empty);

        // Act
        var result = _validator.TestValidate(query);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.OrderId)
            .WithErrorMessage("OrderId is required.");
    }

    [Fact]
    public void Validate_WithDefaultGuid_ShouldHaveError()
    {
        // Arrange
        var query = new GetOrderQuery(default);

        // Act
        var result = _validator.TestValidate(query);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.OrderId)
            .WithErrorMessage("OrderId is required.");
    }
}
