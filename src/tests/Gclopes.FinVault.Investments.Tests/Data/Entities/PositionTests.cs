using FluentAssertions;
using Gclopes.FinVault.Investments.Data;
using Gclopes.FinVault.Investments.Data.Entities;
using Gclopes.FinVault.Investments.DTO;

namespace Gclopes.FinVault.Investments.Tests.Data.Entities;

public class PositionTests
{
    [Fact]
    public void AssetClass_ShouldBeNull_WhenPositionIsCreated()
    {
        // Arrange
        var position = new Position();

        // Assert
        position.AssetClass.Should().BeNull();
    }

    [Fact]
    public void AssetClass_ShouldAcceptCrypto()
    {
        // Arrange
        var position = new Position
        {
            // Act
            AssetClass = AssetClass.Crypto
        };

        // Assert
        position.AssetClass.Should().Be(AssetClass.Crypto);
    }

    [Fact]
    public void PortfolioDto_ShouldExposeAssetClass()
    {
        // Arrange
        var portfolio = new PortfolioDto
        {
            Symbol = "BTC",
            AssetClass = AssetClass.Crypto
        };

        // Assert
        portfolio.AssetClass.Should().Be(AssetClass.Crypto);
    }
}