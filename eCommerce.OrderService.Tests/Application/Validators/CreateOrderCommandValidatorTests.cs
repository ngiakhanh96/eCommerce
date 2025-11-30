using eCommerce.OrderService.Application.Commands;
using eCommerce.OrderService.Application.Validators;
using FluentValidation.TestHelper;

namespace eCommerce.OrderService.Tests.Application.Validators;

/// <summary>
/// Unit tests for CreateOrderCommandValidator.
/// </summary>
public class CreateOrderCommandValidatorTests
{
    private readonly CreateOrderCommandValidator _validator;

    public CreateOrderCommandValidatorTests()
    {
        _validator = new CreateOrderCommandValidator();
    }

    [Fact]
    public void Validate_WithValidCommand_ShouldNotHaveErrors()
    {
        // Arrange
        var command = new CreateOrderCommand
        {
            UserId = Guid.NewGuid(),
            Product = "Laptop",
            Quantity = 2,
            Price = 999.99m
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_WithEmptyUserId_ShouldHaveError()
    {
        // Arrange
        var command = new CreateOrderCommand
        {
            UserId = Guid.Empty,
            Product = "Laptop",
            Quantity = 2,
            Price = 999.99m
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.UserId)
            .WithErrorMessage("UserId is required.");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Validate_WithEmptyProduct_ShouldHaveError(string? product)
    {
        // Arrange
        var command = new CreateOrderCommand
        {
            UserId = Guid.NewGuid(),
            Product = product!,
            Quantity = 2,
            Price = 999.99m
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Product);
    }

    [Fact]
    public void Validate_WithProductExceeding255Characters_ShouldHaveError()
    {
        // Arrange
        var command = new CreateOrderCommand
        {
            UserId = Guid.NewGuid(),
            Product = new string('A', 256),
            Quantity = 2,
            Price = 999.99m
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Product)
            .WithErrorMessage("Product must not exceed 255 characters.");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-100)]
    public void Validate_WithInvalidQuantity_ShouldHaveError(int quantity)
    {
        // Arrange
        var command = new CreateOrderCommand
        {
            UserId = Guid.NewGuid(),
            Product = "Laptop",
            Quantity = quantity,
            Price = 999.99m
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Quantity)
            .WithErrorMessage("Quantity must be greater than 0.");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-999.99)]
    public void Validate_WithInvalidPrice_ShouldHaveError(decimal price)
    {
        // Arrange
        var command = new CreateOrderCommand
        {
            UserId = Guid.NewGuid(),
            Product = "Laptop",
            Quantity = 2,
            Price = price
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Price)
            .WithErrorMessage("Price must be greater than 0.");
    }

    [Fact]
    public void Validate_WithMultipleErrors_ShouldReturnAllErrors()
    {
        // Arrange
        var command = new CreateOrderCommand
        {
            UserId = Guid.Empty,
            Product = "",
            Quantity = 0,
            Price = 0
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.UserId);
        result.ShouldHaveValidationErrorFor(x => x.Product);
        result.ShouldHaveValidationErrorFor(x => x.Quantity);
        result.ShouldHaveValidationErrorFor(x => x.Price);
    }
}
