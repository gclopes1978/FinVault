using System.Text.Json;
using FluentAssertions;
using Gclopes.FinVault.Investments.Data;
using Gclopes.FinVault.Investments.DTO;
using Gclopes.FinVault.Investments.Services.Implementations;
using Gclopes.FinVault.Investments.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;

namespace Gclopes.FinVault.Investments.Tests.Services;

public class Trading212StatementPersistenceTests
{
    [Fact]
    public async Task ImportTrading212StatementAsync_PersistsFullPayloadAndUpsertsBySourceDocumentId()
    {
        var options = new DbContextOptionsBuilder<FinVaultInvestmentsDb>()
            .UseSqlite("Data Source=:memory:")
            .Options;

        await using var dbContext = new FinVaultInvestmentsDb(options);
        await dbContext.Database.OpenConnectionAsync(TestContext.Current.CancellationToken);
        await dbContext.Database.EnsureCreatedAsync(TestContext.Current.CancellationToken);

        var service = new InvestmentsService(
            dbContext,
            Mock.Of<IXtbExcelImporter>(),
            Mock.Of<ILogger<InvestmentsService>>());

        using var firstPayload = JsonDocument.Parse(CreatePayload("AAPL"));
        var firstCount = await service.ImportTrading212StatementAsync(
            new Trading212StatementImportRequest(
                "document-001",
                firstPayload.RootElement.Clone()),
            TestContext.Current.CancellationToken);

        using var updatedPayload = JsonDocument.Parse(CreatePayload("MSFT"));
        var updatedCount = await service.ImportTrading212StatementAsync(
            new Trading212StatementImportRequest(
                "document-001",
                updatedPayload.RootElement.Clone()),
            TestContext.Current.CancellationToken);

        var stored = await dbContext.Trading212PdfStatements
            .SingleAsync(TestContext.Current.CancellationToken);

        firstCount.Should().Be(1);
        updatedCount.Should().Be(1);
        stored.SourceDocumentId.Should().Be("document-001");
        stored.DataJson.Should().Be(updatedPayload.RootElement.GetRawText());
    }

    [Fact]
    public async Task ImportTrading212StatementAsync_RejectsMissingParserSections()
    {
        var options = new DbContextOptionsBuilder<FinVaultInvestmentsDb>()
            .UseSqlite("Data Source=:memory:")
            .Options;

        await using var dbContext = new FinVaultInvestmentsDb(options);
        await dbContext.Database.OpenConnectionAsync(TestContext.Current.CancellationToken);
        await dbContext.Database.EnsureCreatedAsync(TestContext.Current.CancellationToken);

        var service = new InvestmentsService(
            dbContext,
            Mock.Of<IXtbExcelImporter>(),
            Mock.Of<ILogger<InvestmentsService>>());
        using var payload = JsonDocument.Parse("""{"investTransactions":[]}""");

        var act = () => service.ImportTrading212StatementAsync(
            new Trading212StatementImportRequest(
                "document-002",
                payload.RootElement.Clone()),
            TestContext.Current.CancellationToken);

        await act.Should().ThrowAsync<ArgumentException>();
        (await dbContext.Trading212PdfStatements.CountAsync(
            TestContext.Current.CancellationToken)).Should().Be(0);
    }

    private static string CreatePayload(string symbol) =>
        $$"""
        {
          "investTransactions": [{"records": [{"instrument": "{{symbol}}"}]}],
          "investPositions": [],
          "investCashTransactions": [],
          "dividends": [],
          "cryptoTransactions": [],
          "cryptoPositions": [],
          "cryptoCashTransactions": []
        }
        """;
}
