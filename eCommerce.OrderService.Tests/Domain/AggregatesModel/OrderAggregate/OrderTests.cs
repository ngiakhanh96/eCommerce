using eCommerce.OrderService.Domain.AggregatesModel;
using eCommerce.OrderService.Domain.AggregatesModel.OrderAggregate;

namespace eCommerce.OrderService.Tests.Domain.AggregatesModel.OrderAggregate;

public class OrderTests
{
    #region Create Tests

    [Fact]
    public void Create_WithValidParameters_ShouldCreateOrder()
    {
        // Arrange
        var id = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var product = "iPhone 15 Pro";
        var quantity = 2;
        var price = 1999.99m;

        // Act
        var order = Order.Create(id, userId, product, quantity, price);

        // Assert
        Assert.NotNull(order);
        Assert.Equal(id, order.Id);
        Assert.Equal(userId, order.UserId);
        Assert.Equal(product, order.Product);
        Assert.Equal(quantity, order.Quantity);
        Assert.Equal(price, order.Price);
    }

    [Fact]
    public void Create_ShouldSetCreatedAtToUtcNow()
    {
        // Arrange
        var beforeCreation = DateTime.UtcNow;

        // Act
        var order = Order.Create(Guid.NewGuid(), Guid.NewGuid(), "Test", 1, 10m);
        var afterCreation = DateTime.UtcNow;

        // Assert
        Assert.True(order.CreatedAt >= beforeCreation);
        Assert.True(order.CreatedAt <= afterCreation);
    }

    [Fact]
    public void Create_WithEmptyGuid_ShouldCreateOrderWithEmptyId()
    {
        // Arrange & Act
        var order = Order.Create(Guid.Empty, Guid.NewGuid(), "Test", 1, 10m);

        // Assert
        Assert.Equal(Guid.Empty, order.Id);
    }

    [Fact]
    public void Create_WithEmptyUserId_ShouldCreateOrderWithEmptyUserId()
    {
        // Arrange & Act
        var order = Order.Create(Guid.NewGuid(), Guid.Empty, "Test", 1, 10m);

        // Assert
        Assert.Equal(Guid.Empty, order.UserId);
    }

    [Fact]
    public void Create_WithEmptyProduct_ShouldCreateOrderWithEmptyProduct()
    {
        // Arrange & Act
        var order = Order.Create(Guid.NewGuid(), Guid.NewGuid(), string.Empty, 1, 10m);

        // Assert
        Assert.Equal(string.Empty, order.Product);
    }

    [Fact]
    public void Create_WithZeroQuantity_ShouldCreateOrderWithZeroQuantity()
    {
        // Arrange & Act
        var order = Order.Create(Guid.NewGuid(), Guid.NewGuid(), "Test", 0, 10m);

        // Assert
        Assert.Equal(0, order.Quantity);
    }

    [Fact]
    public void Create_WithZeroPrice_ShouldCreateOrderWithZeroPrice()
    {
        // Arrange & Act
        var order = Order.Create(Guid.NewGuid(), Guid.NewGuid(), "Test", 1, 0m);

        // Assert
        Assert.Equal(0m, order.Price);
    }

    [Fact]
    public void Create_WithNegativeQuantity_ShouldCreateOrderWithNegativeQuantity()
    {
        // Note: Domain doesn't enforce validation - that's the application layer's responsibility
        // Arrange & Act
        var order = Order.Create(Guid.NewGuid(), Guid.NewGuid(), "Test", -5, 10m);

        // Assert
        Assert.Equal(-5, order.Quantity);
    }

    [Fact]
    public void Create_WithNegativePrice_ShouldCreateOrderWithNegativePrice()
    {
        // Arrange & Act
        var order = Order.Create(Guid.NewGuid(), Guid.NewGuid(), "Test", 1, -99.99m);

        // Assert
        Assert.Equal(-99.99m, order.Price);
    }

    #endregion

    #region Immutability Tests

    [Fact]
    public void Order_PropertiesArePrivateSet_EnsuresImmutability()
    {
        // Assert - Properties should not have public setters
        var idProperty = typeof(Order).GetProperty(nameof(Order.Id));
        var userIdProperty = typeof(Order).GetProperty(nameof(Order.UserId));
        var productProperty = typeof(Order).GetProperty(nameof(Order.Product));
        var quantityProperty = typeof(Order).GetProperty(nameof(Order.Quantity));
        var priceProperty = typeof(Order).GetProperty(nameof(Order.Price));
        var createdAtProperty = typeof(Order).GetProperty(nameof(Order.CreatedAt));

        Assert.NotNull(idProperty);
        Assert.NotNull(userIdProperty);
        Assert.NotNull(productProperty);
        Assert.NotNull(quantityProperty);
        Assert.NotNull(priceProperty);
        Assert.NotNull(createdAtProperty);

        Assert.False(idProperty.GetSetMethod(false)?.IsPublic ?? false);
        Assert.False(userIdProperty.GetSetMethod(false)?.IsPublic ?? false);
        Assert.False(productProperty.GetSetMethod(false)?.IsPublic ?? false);
        Assert.False(quantityProperty.GetSetMethod(false)?.IsPublic ?? false);
        Assert.False(priceProperty.GetSetMethod(false)?.IsPublic ?? false);
        Assert.False(createdAtProperty.GetSetMethod(false)?.IsPublic ?? false);
    }

    #endregion

    #region Factory Method Pattern Tests

    [Fact]
    public void Create_IsStaticFactoryMethod()
    {
        // Assert
        var createMethod = typeof(Order).GetMethod(nameof(Order.Create));

        Assert.NotNull(createMethod);
        Assert.True(createMethod.IsStatic);
    }

    [Fact]
    public void Order_HasPrivateConstructor_EnforcesFactoryMethodUsage()
    {
        // Assert
        var constructors = typeof(Order).GetConstructors(
            System.Reflection.BindingFlags.NonPublic |
            System.Reflection.BindingFlags.Instance);

        Assert.Contains(constructors, c => c.IsPrivate);
    }

    #endregion

    #region Edge Cases

    [Fact]
    public void Create_MultipleTimes_ShouldCreateIndependentInstances()
    {
        // Arrange
        var id1 = Guid.NewGuid();
        var id2 = Guid.NewGuid();

        // Act
        var order1 = Order.Create(id1, Guid.NewGuid(), "Product 1", 1, 10m);
        var order2 = Order.Create(id2, Guid.NewGuid(), "Product 2", 2, 20m);

        // Assert
        Assert.NotSame(order1, order2);
        Assert.NotEqual(order1.Id, order2.Id);
        Assert.NotEqual(order1.Product, order2.Product);
    }

    [Theory]
    [InlineData("a", 1, 0.01)]
    [InlineData("Very Long Product Name With Many Words", 999, 9999999.99)]
    [InlineData("製品", 1, 100)] // Japanese product name
    [InlineData("Producto Español", 50, 500.50)]
    public void Create_WithVariousValidInputs_ShouldSucceed(string product, int quantity, decimal price)
    {
        // Arrange
        var id = Guid.NewGuid();
        var userId = Guid.NewGuid();

        // Act
        var order = Order.Create(id, userId, product, quantity, price);

        // Assert
        Assert.Equal(product, order.Product);
        Assert.Equal(quantity, order.Quantity);
        Assert.Equal(price, order.Price);
    }

    [Fact]
    public void Create_WithMaxDecimalPrice_ShouldSucceed()
    {
        // Arrange & Act
        var order = Order.Create(Guid.NewGuid(), Guid.NewGuid(), "Expensive", 1, decimal.MaxValue);

        // Assert
        Assert.Equal(decimal.MaxValue, order.Price);
    }

    [Fact]
    public void Create_WithMaxIntQuantity_ShouldSucceed()
    {
        // Arrange & Act
        var order = Order.Create(Guid.NewGuid(), Guid.NewGuid(), "Bulk", int.MaxValue, 1m);

        // Assert
        Assert.Equal(int.MaxValue, order.Quantity);
    }

    #endregion

    #region Aggregate Root Tests

    [Fact]
    public void Order_InheritsFromAggregateRoot()
    {
        // Assert
        Assert.True(typeof(Order).IsSubclassOf(typeof(AggregateRoot)));
    }

    #endregion
}
