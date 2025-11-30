using eCommerce.Mediator.Validation;

namespace eCommerce.Mediator.Tests.Validation;

/// <summary>
/// Unit tests for RequestValidationException.
/// </summary>
public class RequestValidationExceptionTests
{
    [Fact]
    public void Constructor_WithErrorsDictionary_ShouldSetErrors()
    {
        // Arrange
        var errors = new Dictionary<string, string[]>
        {
            { "Name", new[] { "Name is required." } },
            { "Email", new[] { "Email is required.", "Email format is invalid." } }
        };

        // Act
        var exception = new RequestValidationException(errors);

        // Assert
        Assert.Equal(2, exception.Errors.Count);
        Assert.Contains("Name", exception.Errors.Keys);
        Assert.Contains("Email", exception.Errors.Keys);
        Assert.Single(exception.Errors["Name"]);
        Assert.Equal(2, exception.Errors["Email"].Length);
    }

    [Fact]
    public void Constructor_WithErrorsDictionary_ShouldSetDefaultMessage()
    {
        // Arrange
        var errors = new Dictionary<string, string[]>
        {
            { "Field", new[] { "Error message" } }
        };

        // Act
        var exception = new RequestValidationException(errors);

        // Assert
        Assert.Equal("One or more validation errors occurred.", exception.Message);
    }

    [Fact]
    public void Constructor_WithPropertyNameAndMessage_ShouldCreateSingleError()
    {
        // Arrange
        var propertyName = "Username";
        var errorMessage = "Username is required.";

        // Act
        var exception = new RequestValidationException(propertyName, errorMessage);

        // Assert
        Assert.Single(exception.Errors);
        Assert.Contains(propertyName, exception.Errors.Keys);
        Assert.Single(exception.Errors[propertyName]);
        Assert.Equal(errorMessage, exception.Errors[propertyName][0]);
    }

    [Fact]
    public void Constructor_WithPropertyNameAndMessage_ShouldSetDefaultMessage()
    {
        // Act
        var exception = new RequestValidationException("Field", "Error");

        // Assert
        Assert.Equal("One or more validation errors occurred.", exception.Message);
    }

    [Fact]
    public void Errors_ShouldBeReadOnly()
    {
        // Arrange
        var errors = new Dictionary<string, string[]>
        {
            { "Field", new[] { "Error" } }
        };

        // Act
        var exception = new RequestValidationException(errors);

        // Assert
        Assert.IsAssignableFrom<IReadOnlyDictionary<string, string[]>>(exception.Errors);
    }

    [Fact]
    public void Constructor_WithEmptyErrors_ShouldAcceptEmptyDictionary()
    {
        // Arrange
        var errors = new Dictionary<string, string[]>();

        // Act
        var exception = new RequestValidationException(errors);

        // Assert
        Assert.Empty(exception.Errors);
    }

    [Fact]
    public void Constructor_WithMultipleErrorsPerField_ShouldPreserveAllErrors()
    {
        // Arrange
        var errors = new Dictionary<string, string[]>
        {
            { "Password", new[] { "Password is required.", "Password must be at least 8 characters.", "Password must contain a number." } }
        };

        // Act
        var exception = new RequestValidationException(errors);

        // Assert
        Assert.Single(exception.Errors);
        Assert.Equal(3, exception.Errors["Password"].Length);
        Assert.Contains("Password is required.", exception.Errors["Password"]);
        Assert.Contains("Password must be at least 8 characters.", exception.Errors["Password"]);
        Assert.Contains("Password must contain a number.", exception.Errors["Password"]);
    }

    [Fact]
    public void Exception_ShouldInheritFromException()
    {
        // Assert
        Assert.True(typeof(RequestValidationException).IsSubclassOf(typeof(Exception)));
    }

    [Fact]
    public void Exception_ShouldBeCatchableAsException()
    {
        // Arrange
        var errors = new Dictionary<string, string[]>
        {
            { "Field", new[] { "Error" } }
        };

        // Act & Assert
        try
        {
            throw new RequestValidationException(errors);
        }
        catch (Exception ex)
        {
            Assert.IsType<RequestValidationException>(ex);
        }
    }
}
