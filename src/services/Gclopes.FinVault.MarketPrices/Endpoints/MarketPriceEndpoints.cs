using Gclopes.FinVault.MarketPrices.Interfaces;
using Gclopes.FinVault.MarketPrices.Models;

namespace Gclopes.FinVault.MarketPrices.Endpoints;

public static class MarketPricesEndpoints
{
    public static void MapMarketPricesEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/marketprices")
            .WithName("MarketPrices")
            .RequireAuthorization()
            .WithOpenApi();

        group.MapGet("/{assetType}/{symbol}", GetMarketPriceAsync)
            .WithName("GetMarketPrice")
            .WithOpenApi()
            .Produces<AssetQuoteDto>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest);
        group.MapGet("/exchange-rate/{fromCurrency}/{toCurrency}", GetExchangeRateAsync)
            .WithName("GetExchangeRate")
            .WithOpenApi()
            .Produces<decimal>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest);
    }

    private static async Task<IResult> GetExchangeRateAsync(string fromCurrency, string toCurrency, IMarketPriceService service, ILogger<Program> logger, CancellationToken cancellationToken)
    {
        try
        {
            var rate = await service.GetExchangeRateAsync(
                fromCurrency,
                toCurrency,
                cancellationToken);

            return Results.Ok(rate);
        }
        catch (Exception ex)
        {
            logger.LogError(
                ex,
                "Error fetching exchange rate from {FromCurrency} to {ToCurrency}",
                fromCurrency,
                toCurrency);

            return Results.BadRequest(new { message = ex.Message });
        }
    }

    private static async Task<IResult> GetMarketPriceAsync(
        string assetType,
        string symbol,
        IMarketPriceService service,
        ILogger<Program> logger,
        CancellationToken cancellationToken)
    {
        try
        {
            var quote = await service.GetQuoteAsync(
                assetType,
                symbol,
                cancellationToken);

            return Results.Ok(quote);
        }
        catch (Exception ex)
        {
            logger.LogError(
                ex,
                "Error fetching market price for {AssetType} {Symbol}",
                assetType,
                symbol);

            return Results.BadRequest(new { message = ex.Message });
        }
    }
}