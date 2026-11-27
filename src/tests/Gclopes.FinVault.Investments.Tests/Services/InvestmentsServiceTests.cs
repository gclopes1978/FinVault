using FluentAssertions;
using Gclopes.FinVault.Investments.Data;
using Gclopes.FinVault.Investments.Data.Entities;
using Gclopes.FinVault.Investments.Services.Implementations;
using Gclopes.FinVault.Investments.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using System.Globalization;

namespace Gclopes.FinVault.Investments.Tests.Services;

public class InvestmentsServiceTests
{
    [Fact]
    public async Task GetPortfolioAsync_ShouldReturnCryptoPosition_WhenCryptoIsPurchased()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<FinVaultInvestmentsDb>()
            .UseSqlite("Data Source=:memory:")
            .Options;

        await using var dbContext = new FinVaultInvestmentsDb(options);
        await dbContext.Database.OpenConnectionAsync(TestContext.Current.CancellationToken);
        await dbContext.Database.EnsureCreatedAsync(TestContext.Current.CancellationToken);

        dbContext.Trading212CryptoTransactions.Add(
            new Trading212CryptoTransaction
            {
                ExecutionId = "EXEC-001",
                OrderId = "ORDER-001",
                AssetSymbol = "BTC",
                Direction = "Buy",
                Time = new DateTime(2026, 1, 10, 0, 0, 0, DateTimeKind.Utc),
                Quantity = 0.5m,
                ExecutionPrice = 60000m,
                Value = 30000m,
                RealizedProfitLoss = 0m
            });

        await dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var xtbImporter = new Mock<IXtbExcelImporter>();
        var logger = Mock.Of<ILogger<InvestmentsService>>();

        var service = new InvestmentsService(
            dbContext,
            xtbImporter.Object,
            logger);

        // Act
        var result = await service.GetPortfolioAsync(TestContext.Current.CancellationToken);

        // Assert
        result.Should().ContainSingle();

        var position = result.Single();

        position.Symbol.Should().Be("BTC");
        position.Source.Should().Be(BrokerSource.Trading212.ToString());
        position.AssetClass.Should().Be(AssetClass.Crypto);
        position.Quantity.Should().Be(0.5m);
        position.AverageCost.Should().Be(60000m);
        position.CurrentValue.Should().Be(0m);
    }

    [Fact]
    public async Task GetPortfolioAsync_ShouldCalculateRemainingPosition_WhenCryptoIsPartiallySold()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<FinVaultInvestmentsDb>()
            .UseSqlite("Data Source=:memory:")
            .Options;

        await using var dbContext = new FinVaultInvestmentsDb(options);
        var cancellationToken = TestContext.Current.CancellationToken;

        await dbContext.Database.OpenConnectionAsync(cancellationToken);
        await dbContext.Database.EnsureCreatedAsync(cancellationToken);

        dbContext.Trading212CryptoTransactions.AddRange(
            new Trading212CryptoTransaction
            {
                ExecutionId = "EXEC-001",
                OrderId = "ORDER-001",
                AssetSymbol = "BTC",
                Direction = "Buy",
                Time = new DateTime(2026, 1, 10, 0, 0, 0, DateTimeKind.Utc),
                Quantity = 1m,
                ExecutionPrice = 60000m,
                Value = 60000m
            },
            new Trading212CryptoTransaction
            {
                ExecutionId = "EXEC-002",
                OrderId = "ORDER-002",
                AssetSymbol = "BTC",
                Direction = "Sell",
                Time = new DateTime(2026, 2, 10, 0, 0, 0, DateTimeKind.Utc),
                Quantity = 0.4m,
                ExecutionPrice = 70000m,
                Value = 28000m,
                RealizedProfitLoss = 4000m
            });

        await dbContext.SaveChangesAsync(cancellationToken);

        var service = new InvestmentsService(
            dbContext,
            Mock.Of<IXtbExcelImporter>(),
            Mock.Of<ILogger<InvestmentsService>>());

        // Act
        var result = await service.GetPortfolioAsync(cancellationToken);

        // Assert
        result.Should().ContainSingle();

        var position = result.Single();

        position.Symbol.Should().Be("BTC");
        position.AssetClass.Should().Be(AssetClass.Crypto);
        position.Quantity.Should().Be(0.6m);
        position.AverageCost.Should().Be(60000m);
    }

    [Fact]
    public async Task GetPortfolioAsync_ShouldNotReturnPosition_WhenCryptoIsFullySold()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<FinVaultInvestmentsDb>()
            .UseSqlite("Data Source=:memory:")
            .Options;

        await using var dbContext = new FinVaultInvestmentsDb(options);
        var cancellationToken = TestContext.Current.CancellationToken;

        await dbContext.Database.OpenConnectionAsync(cancellationToken);
        await dbContext.Database.EnsureCreatedAsync(cancellationToken);

        dbContext.Trading212CryptoTransactions.AddRange(
            new Trading212CryptoTransaction
            {
                ExecutionId = "EXEC-001",
                OrderId = "ORDER-001",
                AssetSymbol = "BTC",
                Direction = "Buy",
                Time = new DateTime(2026, 1, 10, 0, 0, 0, DateTimeKind.Utc),
                Quantity = 0.5m,
                ExecutionPrice = 60000m,
                Value = 30000m
            },
            new Trading212CryptoTransaction
            {
                ExecutionId = "EXEC-002",
                OrderId = "ORDER-002",
                AssetSymbol = "BTC",
                Direction = "Sell",
                Time = new DateTime(2026, 2, 10, 0, 0, 0, DateTimeKind.Utc),
                Quantity = 0.5m,
                ExecutionPrice = 70000m,
                Value = 35000m,
                RealizedProfitLoss = 5000m
            });

        await dbContext.SaveChangesAsync(cancellationToken);

        var service = new InvestmentsService(
            dbContext,
            Mock.Of<IXtbExcelImporter>(),
            Mock.Of<ILogger<InvestmentsService>>());

        // Act
        var result = await service.GetPortfolioAsync(cancellationToken);

        // Assert
        result.Should().BeEmpty();
    }
    [Fact]
    public async Task GetPortfolioAsync_ShouldReturnIndependentPositions_WhenMultipleCryptosExist()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<FinVaultInvestmentsDb>()
            .UseSqlite("Data Source=:memory:")
            .Options;

        await using var dbContext = new FinVaultInvestmentsDb(options);
        var cancellationToken = TestContext.Current.CancellationToken;

        await dbContext.Database.OpenConnectionAsync(cancellationToken);
        await dbContext.Database.EnsureCreatedAsync(cancellationToken);

        dbContext.Trading212CryptoTransactions.AddRange(
            new Trading212CryptoTransaction
            {
                ExecutionId = "EXEC-001",
                OrderId = "ORDER-001",
                AssetSymbol = "BTC",
                Direction = "Buy",
                Time = new DateTime(2026, 1, 10, 0, 0, 0, DateTimeKind.Utc),
                Quantity = 0.5m,
                ExecutionPrice = 60000m,
                Value = 30000m
            },
            new Trading212CryptoTransaction
            {
                ExecutionId = "EXEC-002",
                OrderId = "ORDER-002",
                AssetSymbol = "ETH",
                Direction = "Buy",
                Time = new DateTime(2026, 1, 11, 0, 0, 0, DateTimeKind.Utc),
                Quantity = 2m,
                ExecutionPrice = 3000m,
                Value = 6000m
            });

        await dbContext.SaveChangesAsync(cancellationToken);

        var service = new InvestmentsService(
            dbContext,
            Mock.Of<IXtbExcelImporter>(),
            Mock.Of<ILogger<InvestmentsService>>());

        // Act
        var result = await service.GetPortfolioAsync(cancellationToken);

        // Assert
        result.Should().HaveCount(2);

        var bitcoin = result.Single(p => p.Symbol == "BTC");
        bitcoin.AssetClass.Should().Be(AssetClass.Crypto);
        bitcoin.Quantity.Should().Be(0.5m);
        bitcoin.AverageCost.Should().Be(60000m);

        var ethereum = result.Single(p => p.Symbol == "ETH");
        ethereum.AssetClass.Should().Be(AssetClass.Crypto);
        ethereum.Quantity.Should().Be(2m);
        ethereum.AverageCost.Should().Be(3000m);
    }

    [Fact]
    public async Task GetPortfolioAsync_ShouldCalculateWeightedAverageCost_WhenMultiplePurchasesHaveDifferentPrices()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<FinVaultInvestmentsDb>()
            .UseSqlite("Data Source=:memory:")
            .Options;

        await using var dbContext = new FinVaultInvestmentsDb(options);
        var cancellationToken = TestContext.Current.CancellationToken;

        await dbContext.Database.OpenConnectionAsync(cancellationToken);
        await dbContext.Database.EnsureCreatedAsync(cancellationToken);

        dbContext.Trading212CryptoTransactions.AddRange(
            new Trading212CryptoTransaction
            {
                ExecutionId = "EXEC-001",
                OrderId = "ORDER-001",
                AssetSymbol = "BTC",
                Direction = "Buy",
                Time = new DateTime(2026, 1, 10, 0, 0, 0, DateTimeKind.Utc),
                Quantity = 1m,
                ExecutionPrice = 60000m,
                Value = 60000m
            },
            new Trading212CryptoTransaction
            {
                ExecutionId = "EXEC-002",
                OrderId = "ORDER-002",
                AssetSymbol = "BTC",
                Direction = "Buy",
                Time = new DateTime(2026, 2, 10, 0, 0, 0, DateTimeKind.Utc),
                Quantity = 1m,
                ExecutionPrice = 80000m,
                Value = 80000m
            },
            new Trading212CryptoTransaction
            {
                ExecutionId = "EXEC-003",
                OrderId = "ORDER-003",
                AssetSymbol = "BTC",
                Direction = "Sell",
                Time = new DateTime(2026, 3, 10, 0, 0, 0, DateTimeKind.Utc),
                Quantity = 0.5m,
                ExecutionPrice = 90000m,
                Value = 45000m
            });

        await dbContext.SaveChangesAsync(cancellationToken);

        var service = new InvestmentsService(
            dbContext,
            Mock.Of<IXtbExcelImporter>(),
            Mock.Of<ILogger<InvestmentsService>>());

        // Act
        var result = await service.GetPortfolioAsync(cancellationToken);

        // Assert
        result.Should().ContainSingle();

        var position = result.Single();

        position.Quantity.Should().Be(1.5m);
        position.AverageCost.Should().Be(70000m);
        position.AssetClass.Should().Be(AssetClass.Crypto);
    }
}