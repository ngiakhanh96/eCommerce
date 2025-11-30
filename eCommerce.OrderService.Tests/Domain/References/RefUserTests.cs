using eCommerce.OrderService.Domain.References;

namespace eCommerce.OrderService.Tests.Domain.References;

public class RefUserTests
{
    #region Property Tests

    [Fact]
    public void RefUser_CanSetAndGetAllProperties()
    {
        // Arrange
        var id = Guid.NewGuid();
        var name = "John Doe";
        var email = "john.doe@example.com";
        var createdAt = DateTime.UtcNow;
        var updatedAt = DateTime.UtcNow.AddMinutes(5);

        // Act
        var refUser = new RefUser
        {
            Id = id,
            Name = name,
            Email = email,
            CreatedAt = createdAt,
            UpdatedAt = updatedAt
        };

        // Assert
        Assert.Equal(id, refUser.Id);
        Assert.Equal(name, refUser.Name);
        Assert.Equal(email, refUser.Email);
        Assert.Equal(createdAt, refUser.CreatedAt);
        Assert.Equal(updatedAt, refUser.UpdatedAt);
    }

    [Fact]
    public void RefUser_UpdatedAtCanBeNull()
    {
        // Arrange & Act
        var refUser = new RefUser
        {
            Id = Guid.NewGuid(),
            Name = "Test",
            Email = "test@example.com",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = null
        };

        // Assert
        Assert.Null(refUser.UpdatedAt);
    }

    #endregion

    #region Default Value Tests

    [Fact]
    public void RefUser_DefaultName_IsEmptyString()
    {
        // Arrange & Act
        var refUser = new RefUser();

        // Assert
        Assert.Equal(string.Empty, refUser.Name);
    }

    [Fact]
    public void RefUser_DefaultEmail_IsEmptyString()
    {
        // Arrange & Act
        var refUser = new RefUser();

        // Assert
        Assert.Equal(string.Empty, refUser.Email);
    }

    [Fact]
    public void RefUser_DefaultId_IsEmptyGuid()
    {
        // Arrange & Act
        var refUser = new RefUser();

        // Assert
        Assert.Equal(Guid.Empty, refUser.Id);
    }

    [Fact]
    public void RefUser_DefaultCreatedAt_IsMinValue()
    {
        // Arrange & Act
        var refUser = new RefUser();

        // Assert
        Assert.Equal(default(DateTime), refUser.CreatedAt);
    }

    [Fact]
    public void RefUser_DefaultUpdatedAt_IsNull()
    {
        // Arrange & Act
        var refUser = new RefUser();

        // Assert
        Assert.Null(refUser.UpdatedAt);
    }

    #endregion

    #region Edge Cases

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("A")]
    [InlineData("Very Long Name With Many Words And Characters That Could Be A Full Name")]
    [InlineData("名前")] // Japanese
    [InlineData("Ñoño García")] // Spanish with special chars
    [InlineData("O'Connor")] // Irish name with apostrophe
    public void RefUser_WithVariousNames_ShouldAcceptValue(string name)
    {
        // Arrange & Act
        var refUser = new RefUser { Name = name };

        // Assert
        Assert.Equal(name, refUser.Name);
    }

    [Theory]
    [InlineData("")]
    [InlineData("a@b.c")]
    [InlineData("very.long.email.address@subdomain.example.com")]
    [InlineData("user+tag@example.com")]
    [InlineData("invalid-email")] // Domain doesn't validate format
    public void RefUser_WithVariousEmails_ShouldAcceptValue(string email)
    {
        // Arrange & Act
        var refUser = new RefUser { Email = email };

        // Assert
        Assert.Equal(email, refUser.Email);
    }

    #endregion

    #region Mutability Tests

    [Fact]
    public void RefUser_CanUpdateProperties()
    {
        // Arrange
        var refUser = new RefUser
        {
            Id = Guid.NewGuid(),
            Name = "Original Name",
            Email = "original@example.com",
            CreatedAt = DateTime.UtcNow
        };

        // Act
        refUser.Name = "Updated Name";
        refUser.Email = "updated@example.com";
        refUser.UpdatedAt = DateTime.UtcNow;

        // Assert
        Assert.Equal("Updated Name", refUser.Name);
        Assert.Equal("updated@example.com", refUser.Email);
        Assert.NotNull(refUser.UpdatedAt);
    }

    #endregion
}
