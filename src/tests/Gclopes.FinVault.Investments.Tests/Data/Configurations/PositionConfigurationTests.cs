using FluentAssertions;
using Gclopes.FinVault.Investments.Data;
using Gclopes.FinVault.Investments.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace Gclopes.FinVault.Investments.Tests.Data.Configurations;

public class PositionConfigurationTests
{
    [Fact]
    public void Position_ShouldHaveUniqueIndexIncludingAssetClass()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<FinVaultInvestmentsDb>()
            .UseSqlServer("Server=(localdb)\\MSSQLLocalDB;Database=FinVaultTests")
            .Options;

        using var context = new FinVaultInvestmentsDb(options);

        // Act
        var entityType = context.Model.FindEntityType(typeof(Position));
        var uniqueIndex = entityType!.GetIndexes()
            .Single(index => index.IsUnique);

        var propertyNames = uniqueIndex.Properties
            .Select(property => property.Name)
            .ToArray();

        // Assert
        propertyNames.Should().BeEquivalentTo(
            "Symbol",
            "Source",
            "AssetClass");
    }
}