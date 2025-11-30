using eCommerce.UserService.Domain.AggregatesModel;
using eCommerce.UserService.Domain.AggregatesModel.UserAggregate;

namespace eCommerce.UserService.Tests.Domain.AggregatesModel.UserAggregate;

public class UserTests
{
    #region Create Tests

    [Fact]
    public void Create_WithValidParameters_ShouldCreateUser()
    {
        // Arrange
        var id = Guid.NewGuid();
        var name = "John Doe";
        var email = "john.doe@example.com";

        // Act
        var user = User.Create(id, name, email);

        // Assert
        Assert.NotNull(user);
        Assert.Equal(id, user.Id);
        Assert.Equal(name, user.Name);
        Assert.Equal(email, user.Email);
    }

    [Fact]
    public void Create_ShouldSetCreatedAtToUtcNow()
    {
        // Arrange
        var id = Guid.NewGuid();
        var beforeCreation = DateTime.UtcNow;

        // Act
        var user = User.Create(id, "Test User", "test@example.com");
        var afterCreation = DateTime.UtcNow;

        // Assert
        Assert.True(user.CreatedAt >= beforeCreation);
        Assert.True(user.CreatedAt <= afterCreation);
    }

    [Fact]
    public void Create_WithEmptyGuid_ShouldCreateUserWithEmptyId()
    {
        // Arrange
        var id = Guid.Empty;

        // Act
        var user = User.Create(id, "Test", "test@example.com");

        // Assert
        Assert.Equal(Guid.Empty, user.Id);
    }

    [Fact]
    public void Create_WithEmptyName_ShouldCreateUserWithEmptyName()
    {
        // Arrange & Act
        var user = User.Create(Guid.NewGuid(), string.Empty, "test@example.com");

        // Assert
        Assert.Equal(string.Empty, user.Name);
    }

    [Fact]
    public void Create_WithEmptyEmail_ShouldCreateUserWithEmptyEmail()
    {
        // Arrange & Act
        var user = User.Create(Guid.NewGuid(), "Test", string.Empty);

        // Assert
        Assert.Equal(string.Empty, user.Email);
    }

    [Fact]
    public void Create_WithWhitespaceName_ShouldPreserveWhitespace()
    {
        // Arrange
        var name = "   ";

        // Act
        var user = User.Create(Guid.NewGuid(), name, "test@example.com");

        // Assert
        Assert.Equal(name, user.Name);
    }

    #endregion

    #region Immutability Tests

    [Fact]
    public void User_PropertiesArePrivateSet_EnsuresImmutability()
    {
        // Arrange
        var user = User.Create(Guid.NewGuid(), "Test", "test@example.com");

        // Assert - Properties should not have public setters
        var idProperty = typeof(User).GetProperty(nameof(User.Id));
        var nameProperty = typeof(User).GetProperty(nameof(User.Name));
        var emailProperty = typeof(User).GetProperty(nameof(User.Email));
        var createdAtProperty = typeof(User).GetProperty(nameof(User.CreatedAt));

        Assert.NotNull(idProperty);
        Assert.NotNull(nameProperty);
        Assert.NotNull(emailProperty);
        Assert.NotNull(createdAtProperty);

        Assert.False(idProperty.GetSetMethod(false)?.IsPublic ?? false);
        Assert.False(nameProperty.GetSetMethod(false)?.IsPublic ?? false);
        Assert.False(emailProperty.GetSetMethod(false)?.IsPublic ?? false);
        Assert.False(createdAtProperty.GetSetMethod(false)?.IsPublic ?? false);
    }

    #endregion

    #region Factory Method Pattern Tests

    [Fact]
    public void Create_IsStaticFactoryMethod()
    {
        // Assert
        var createMethod = typeof(User).GetMethod(nameof(User.Create));

        Assert.NotNull(createMethod);
        Assert.True(createMethod.IsStatic);
    }

    [Fact]
    public void User_HasPrivateConstructor_EnforcesFactoryMethodUsage()
    {
        // Assert
        var constructors = typeof(User).GetConstructors(
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
        var user1 = User.Create(id1, "User 1", "user1@example.com");
        var user2 = User.Create(id2, "User 2", "user2@example.com");

        // Assert
        Assert.NotSame(user1, user2);
        Assert.NotEqual(user1.Id, user2.Id);
        Assert.NotEqual(user1.Name, user2.Name);
        Assert.NotEqual(user1.Email, user2.Email);
    }

    [Theory]
    [InlineData("a", "a@b.c")]
    [InlineData("Very Long Name With Many Words And Characters", "very.long.email.address@subdomain.example.com")]
    [InlineData("名前", "japanese@example.jp")]
    [InlineData("Ñoño", "spanish@example.es")]
    public void Create_WithVariousValidInputs_ShouldSucceed(string name, string email)
    {
        // Arrange
        var id = Guid.NewGuid();

        // Act
        var user = User.Create(id, name, email);

        // Assert
        Assert.Equal(name, user.Name);
        Assert.Equal(email, user.Email);
    }

    #endregion

    #region Aggregate Root Tests

    [Fact]
    public void User_InheritsFromAggregateRoot()
    {
        // Assert
        Assert.True(typeof(User).IsSubclassOf(typeof(AggregateRoot)));
    }

    #endregion
}
