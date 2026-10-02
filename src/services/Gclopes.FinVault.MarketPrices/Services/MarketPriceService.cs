using Gclopes.FinVault.MarketPrices.Interfaces;
using Gclopes.FinVault.MarketPrices.Models;

namespace Gclopes.FinVault.MarketPrices.Services;

public class MarketPriceService(IMarketPriceResolver resolver) : IMarketPriceService
{
    private readonly IMarketPriceResolver _resolver = resolver;

    public async Task<decimal> GetExchangeRateAsync(string fromCurrency, string toCurrency, CancellationToken cancellationToken)
    {
        if (fromCurrency.Equals(
                toCurrency,
                StringComparison.OrdinalIgnoreCase))
        {
            return 1m;
        }

        
        if (fromCurrency.Equals(
                "USD",
                StringComparison.OrdinalIgnoreCase))
        {
            var usdToTargetQuote = await GetQuoteAsync(
                "ETF",
                $"{toCurrency}USD=X",
                cancellationToken);

            if (usdToTargetQuote.Price <= 0)
            {
                throw new InvalidOperationException(
                    $"Invalid exchange rate for {fromCurrency}/{toCurrency}.");
            }

            return 1m / usdToTargetQuote.Price;
        }

        var fromUsdQuote = await GetQuoteAsync(
            "ETF",
            $"{fromCurrency}USD=X",
            cancellationToken);

        var toUsdQuote = await GetQuoteAsync(
            "ETF",
            $"{toCurrency}USD=X",
            cancellationToken);

        if (fromUsdQuote.Price <= 0 || toUsdQuote.Price <= 0)
        {
            throw new InvalidOperationException(
                $"Invalid exchange rate for {fromCurrency}/{toCurrency}.");
        }

        return fromUsdQuote.Price / toUsdQuote.Price;
    }

    public async Task<AssetQuoteDto> GetQuoteAsync(
        string assetType,
        string symbol,
        CancellationToken cancellationToken)
    {
        Console.WriteLine(
            $"Getting market price for {assetType} - {symbol}");

        var resolvedSymbol = assetType.Equals("crypto", StringComparison.CurrentCultureIgnoreCase) ? symbol : _resolver.ResolveYahooSymbol(symbol) ?? (symbol.Contains('.') ? symbol : await _resolver.ResolveTickerFromIsinAsync(symbol));
        Console.WriteLine($"Resolved market symbol: {symbol} -> {resolvedSymbol}");
        if (string.IsNullOrWhiteSpace(resolvedSymbol))
        {
            throw new Exception(
                $"Unable to resolve ticker for symbol/ISIN '{symbol}'");
        }

        var provider = _resolver.GetProvider(assetType);

        var quote = await provider.GetQuoteAsync(
            resolvedSymbol,
            cancellationToken);

        if (quote.Currency.Equals("GBp", StringComparison.OrdinalIgnoreCase))
        {
            return quote with
            {
                Price = quote.Price / 100,
                Currency = "GBP"
            };
        }

        return quote;
    }
}