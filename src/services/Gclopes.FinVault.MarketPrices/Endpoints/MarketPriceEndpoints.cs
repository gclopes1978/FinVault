using Gclopes.FinVault.MarketPrices.Interfaces;

namespace Gclopes.FinVault.MarketPrices.Endpoints;

public static class MarketPricesEndpoints
{
    public static void MapMarketPricesEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/marketprices")
            .WithName("MarketPrices")
            .RequireAuthorization() 
            .WithOpenApi();

        group.MapGet("/{assetType}/{symbol}", GetRawPriceAsync)
            .WithName("GetRawMarketPrice")
            .WithOpenApi()
            .Produces<string>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest);
    }

    private static async Task<IResult> GetRawPriceAsync(
        string assetType,
        string symbol,
        IMarketPriceService service,
        ILogger<Program> logger,
        CancellationToken cancellationToken)
    {
        try
        {
            var json = await service.GetRawAsync(assetType, symbol, cancellationToken);
            return Results.Ok(json);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error fetching market price for {AssetType} {Symbol}", assetType, symbol);
            return Results.BadRequest(new { message = ex.Message });
        }
    }
}
