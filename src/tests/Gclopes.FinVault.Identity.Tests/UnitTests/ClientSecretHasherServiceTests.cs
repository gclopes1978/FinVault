using FluentAssertions;
using Gclopes.FinVault.Identity.Services;
using Xunit;

namespace Gclopes.FinVault.Identity.Tests.UnitTests;

public class ClientSecretHasherServiceTests
{
    private readonly ClientSecretHasherService _hasher = new();

    [Fact]
    public void HashSecret_ShouldGenerateDifferentHashes_ForSameSecret()
    {
        // Arrange
        const string secret = "my-super-secret-client-key";

        // Act
        var firstHash = _hasher.HashSecret(secret);
        var secondHash = _hasher.HashSecret(secret);

        // Assert
        firstHash.Should().NotBe(secondHash);
    }

    [Fact]
    public void VerifySecret_ShouldReturnTrue_WhenSecretIsCorrect()
    {
        // Arrange
        const string secret = "my-super-secret-client-key";
        var hash = _hasher.HashSecret(secret);

        // Act
        var result = _hasher.VerifySecret(secret, hash);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void VerifySecret_ShouldReturnFalse_WhenSecretIsIncorrect()
    {
        // Arrange
        var hash = _hasher.HashSecret("correct-secret");

        // Act
        var result = _hasher.VerifySecret("incorrect-secret", hash);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void VerifySecret_ShouldReturnFalse_WhenHashIsMalformed()
    {
        // Act
        var result = _hasher.VerifySecret(
            "some-secret",
            "invalid-hash");

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void VerifySecret_ShouldReturnFalse_WhenSecretIsEmpty()
    {
        // Act
        var result = _hasher.VerifySecret(
            string.Empty,
            "some-hash");

        // Assert
        result.Should().BeFalse();
    }
}