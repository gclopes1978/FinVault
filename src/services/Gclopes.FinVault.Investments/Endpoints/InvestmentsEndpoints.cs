using Gclopes.FinVault.Investments.Services.Interfaces;

namespace Gclopes.FinVault.Investments.Endpoints;

public static class InvestmentsEndpoints
{
    public static void MapInvestmentsEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/investments")
            .WithName("Investments")
            .WithOpenApi();

        group.MapPost("/import/xtb", ImportXtbAsync)
            .WithName("ImportXtbCsv")
            .DisableAntiforgery()
            .WithOpenApi()
            .Produces<ImportCsvResponse>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest);

        group.MapPost("/import/trading212", ImportTrading212Async)
            .WithName("ImportTrading212Csv")
            .DisableAntiforgery()
            .WithOpenApi()
            .Produces<ImportCsvResponse>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest);

        group.MapGet("/portfolio", GetPortfolioAsync)
            .WithName("GetPortfolio")
            .WithOpenApi()
            .Produces<List<object>>(StatusCodes.Status200OK);

        group.MapGet("/portfolio/history", GetPortfolioHistoryAsync)
            .WithName("GetPortfolioHistory")
            .WithOpenApi()
            .Produces<List<object>>(StatusCodes.Status200OK);
    }

    private static async Task<IResult> ImportXtbAsync(
        IFormFile file,
        IInvestmentsService investmentsService,
        ILogger<Program> logger,
        CancellationToken cancellationToken)
    {
        // Chamamos o método auxiliar e passamos o método específico da XTB
        return await HandleCsvImportAsync(
            file,
            investmentsService.ImportXtbCsvAsync,
            "XTB",
            logger,
            cancellationToken);
    }

    private static async Task<IResult> ImportTrading212Async(
        IFormFile file,
        IInvestmentsService investmentsService,
        ILogger<Program> logger,
        CancellationToken cancellationToken)
    {
        // Chamamos o mesmo método auxiliar mas passamos o método da T212
        return await HandleCsvImportAsync(
            file,
            investmentsService.ImportTrading212CsvAsync,
            "Trading212",
            logger,
            cancellationToken);
    }

    private static async Task<IResult> HandleCsvImportAsync(
        IFormFile file,
        Func<Stream, CancellationToken, Task<int>> importFunction,
        string sourceName,
        ILogger<Program> logger,
        CancellationToken cancellationToken)
    {
        if (file == null || file.Length == 0)
        {
            return Results.BadRequest(new { message = "No file uploaded" });
        }

        try
        {
            using var stream = file.OpenReadStream();

            // Executa a função que lhe foi passada (seja XTB ou T212)
            var importedCount = await importFunction(stream, cancellationToken);

            return Results.Ok(new ImportCsvResponse { ImportedRecords = importedCount });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error importing {SourceName} CSV", sourceName);
            return Results.BadRequest(new { message = ex.Message });
        }
    }

    private static async Task<IResult> GetPortfolioAsync(
        IInvestmentsService investmentsService,
        CancellationToken cancellationToken)
    {
        var portfolio = await investmentsService.GetPortfolioAsync(cancellationToken);
        return Results.Ok(portfolio);
    }

    private static async Task<IResult> GetPortfolioHistoryAsync(
        IInvestmentsService investmentsService,
        CancellationToken cancellationToken)
    {
        var history = await investmentsService.GetPortfolioHistoryAsync(cancellationToken);
        return Results.Ok(history);
    }
    
}

public class ImportCsvResponse
{
    public int ImportedRecords { get; set; }
}