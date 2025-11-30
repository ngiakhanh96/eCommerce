using eCommerce.UserService.Domain.References;

namespace eCommerce.UserService.Tests.Domain.References;

public class RefOrderTests
{
    #region Property Tests

    [Fact]
    public void RefOrder_CanSetAndGetAllProperties()
    {
        // Arrange
        var id = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var product = "Test Product";
        var quantity = 5;
        var price = 99.99m;
        var createdAt = DateTime.UtcNow;
        var updatedAt = DateTime.UtcNow.AddMinutes(5);

        // Act
        var refOrder = new RefOrder
        {
            Id = id,
            UserId = userId,
            Product = product,
            Quantity = quantity,
            Price = price,
            CreatedAt = createdAt,
            UpdatedAt = updatedAt
        };

        // Assert
        Assert.Equal(id, refOrder.Id);
        Assert.Equal(userId, refOrder.UserId);
        Assert.Equal(product, refOrder.Product);
        Assert.Equal(quantity, refOrder.Quantity);
        Assert.Equal(price, refOrder.Price);
        Assert.Equal(createdAt, refOrder.CreatedAt);
        Assert.Equal(updatedAt, refOrder.UpdatedAt);
    }

    [Fact]
    public void RefOrder_UpdatedAtCanBeNull()
    {
        // Arrange & Act
        var refOrder = new RefOrder
        {
            Id = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            Product = "Test",
            Quantity = 1,
            Price = 10m,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = null
        };

        // Assert
        Assert.Null(refOrder.UpdatedAt);
    }

    #endregion

    #region Default Value Tests

    [Fact]
    public void RefOrder_DefaultProduct_IsEmptyString()
    {
        // Arrange & Act
        var refOrder = new RefOrder();

        // Assert
        Assert.Equal(string.Empty, refOrder.Product);
    }

    [Fact]
    public void RefOrder_DefaultId_IsEmptyGuid()
    {
        // Arrange & Act
        var refOrder = new RefOrder();

        // Assert
        Assert.Equal(Guid.Empty, refOrder.Id);
    }

    [Fact]
    public void RefOrder_DefaultQuantity_IsZero()
    {
        // Arrange & Act
        var refOrder = new RefOrder();

        // Assert
        Assert.Equal(0, refOrder.Quantity);
    }

    [Fact]
    public void RefOrder_DefaultPrice_IsZero()
    {
        // Arrange & Act
        var refOrder = new RefOrder();

        // Assert
        Assert.Equal(0m, refOrder.Price);
    }

    #endregion

    #region Edge Cases

    [Fact]
    public void RefOrder_WithNegativeQuantity_ShouldAcceptValue()
    {
        // Arrange & Act (domain doesn't enforce validation - that's application layer)
        var refOrder = new RefOrder { Quantity = -1 };

        // Assert
        Assert.Equal(-1, refOrder.Quantity);
    }

    [Fact]
    public void RefOrder_WithNegativePrice_ShouldAcceptValue()
    {
        // Arrange & Act
        var refOrder = new RefOrder { Price = -99.99m };

        // Assert
        Assert.Equal(-99.99m, refOrder.Price);
    }

    [Fact]
    public void RefOrder_WithMaxDecimalPrice_ShouldAcceptValue()
    {
        // Arrange & Act
        var refOrder = new RefOrder { Price = decimal.MaxValue };

        // Assert
        Assert.Equal(decimal.MaxValue, refOrder.Price);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("Simple Product")]
    [InlineData("Product with special chars: @#$%")]
    [InlineData("製品名")] // Japanese
    public void RefOrder_WithVariousProductNames_ShouldAcceptValue(string product)
    {
        // Arrange & Act
        var refOrder = new RefOrder { Product = product };

        // Assert
        Assert.Equal(product, refOrder.Product);
    }

    #endregion
}
