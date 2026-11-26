using FluentAssertions;
using Gclopes.FinVault.Investments.Data;
using Microsoft.EntityFrameworkCore;

namespace Gclopes.FinVault.Investments.Tests.Data.Configurations;

public class Trading212CryptoTransactionConfigurationTests
{
    [Fact]
    public void Trading212CryptoTransaction_ShouldHaveUniqueExecutionIdIndex()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<FinVaultInvestmentsDb>()
            .UseSqlServer("Server=(localdb)\\MSSQLLocalDB;Database=FinVaultTests")
            .Options;

        using var context = new FinVaultInvestmentsDb(options);

        // Act
        var entityType = context.Model.FindEntityType(
            "Gclopes.FinVault.Investments.Data.Entities.Trading212CryptoTransaction");

        // Assert
        entityType.Should().NotBeNull();

        var executionIdIndex = entityType.GetIndexes()
            .Single(index => index.Properties.Any(
                property => property.Name == "ExecutionId"));

        executionIdIndex.IsUnique.Should().BeTrue();
    }
}