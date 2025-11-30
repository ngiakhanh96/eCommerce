using eCommerce.EventBus.Resilience;
using Microsoft.Extensions.Logging;
using Polly.CircuitBreaker;

namespace eCommerce.EventBus.Tests.Resilience;

/// <summary>
/// Unit tests for ResiliencePolicies.
/// </summary>
public class ResiliencePoliciesTests
{
    [Fact]
    public void CreateRetryPolicy_ShouldReturnNonNullPolicy()
    {
        // Arrange
        var mockLogger = new Mock<ILogger>();

        // Act
        var policy = ResiliencePolicies.CreateRetryPolicy(mockLogger.Object);

        // Assert
        Assert.NotNull(policy);
    }

    [Fact]
    public async Task CreateRetryPolicy_ShouldRetryOnException()
    {
        // Arrange
        var mockLogger = new Mock<ILogger>();
        var policy = ResiliencePolicies.CreateRetryPolicy(mockLogger.Object);
        var callCount = 0;

        // Act
        await policy.ExecuteAsync(() =>
        {
            callCount++;
            if (callCount < 3)
            {
                throw new Exception("Transient error");
            }
            return Task.CompletedTask;
        });

        // Assert
        Assert.Equal(3, callCount);
    }

    [Fact]
    public async Task CreateRetryPolicy_ShouldThrowAfterMaxRetries()
    {
        // Arrange
        var mockLogger = new Mock<ILogger>();
        var policy = ResiliencePolicies.CreateRetryPolicy(mockLogger.Object);
        var callCount = 0;

        // Act & Assert
        await Assert.ThrowsAsync<Exception>(async () =>
        {
            await policy.ExecuteAsync(() =>
            {
                callCount++;
                throw new Exception("Persistent error");
            });
        });

        // Should be called 4 times (1 initial + 3 retries)
        Assert.Equal(4, callCount);
    }

    [Fact]
    public void CreateCircuitBreakerPolicy_ShouldReturnNonNullPolicy()
    {
        // Arrange
        var mockLogger = new Mock<ILogger>();

        // Act
        var policy = ResiliencePolicies.CreateCircuitBreakerPolicy(mockLogger.Object);

        // Assert
        Assert.NotNull(policy);
    }

    [Fact]
    public async Task CreateCircuitBreakerPolicy_ShouldOpenAfterConsecutiveFailures()
    {
        // Arrange
        var mockLogger = new Mock<ILogger>();
        var policy = ResiliencePolicies.CreateCircuitBreakerPolicy(mockLogger.Object);

        // Act - Cause 5 consecutive failures to open the circuit
        for (int i = 0; i < 5; i++)
        {
            try
            {
                await policy.ExecuteAsync(() => throw new Exception("Failure"));
            }
            catch (Exception)
            {
                // Expected
            }
        }

        // Assert - The 6th call should throw BrokenCircuitException
        await Assert.ThrowsAsync<BrokenCircuitException>(async () =>
        {
            await policy.ExecuteAsync(() => Task.CompletedTask);
        });
    }

    [Fact]
    public void CreateResilientPolicy_ShouldReturnNonNullPolicy()
    {
        // Arrange
        var mockLogger = new Mock<ILogger>();

        // Act
        var policy = ResiliencePolicies.CreateResilientPolicy(mockLogger.Object);

        // Assert
        Assert.NotNull(policy);
    }

    [Fact]
    public async Task CreateResilientPolicy_ShouldRetryBeforeCircuitBreaker()
    {
        // Arrange
        var mockLogger = new Mock<ILogger>();
        var policy = ResiliencePolicies.CreateResilientPolicy(mockLogger.Object);
        var callCount = 0;

        // Act
        await policy.ExecuteAsync(() =>
        {
            callCount++;
            if (callCount < 3)
            {
                throw new Exception("Transient error");
            }
            return Task.CompletedTask;
        });

        // Assert - Should succeed after retries
        Assert.Equal(3, callCount);
    }

    [Fact]
    public async Task CreateResilientPolicy_ShouldSucceedOnFirstTry()
    {
        // Arrange
        var mockLogger = new Mock<ILogger>();
        var policy = ResiliencePolicies.CreateResilientPolicy(mockLogger.Object);
        var callCount = 0;

        // Act
        await policy.ExecuteAsync(() =>
        {
            callCount++;
            return Task.CompletedTask;
        });

        // Assert
        Assert.Equal(1, callCount);
    }

    [Fact]
    public async Task CreateRetryPolicy_ShouldLogOnRetry()
    {
        // Arrange
        var mockLogger = new Mock<ILogger>();
        var policy = ResiliencePolicies.CreateRetryPolicy(mockLogger.Object);
        var callCount = 0;

        // Act
        await policy.ExecuteAsync(() =>
        {
            callCount++;
            if (callCount < 2)
            {
                throw new Exception("Retry error");
            }
            return Task.CompletedTask;
        });

        // Assert - Verify logger was called for retry
        mockLogger.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Retry attempt")),
                It.IsAny<Exception?>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.AtLeastOnce);
    }
}
