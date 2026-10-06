namespace Gclopes.FinVault.Investments.Services.Interfaces;

public interface ITrading212CsvImporter
{
    Task<List<Trading212CsvRecord>> ParseCsvAsync(
        Stream csvStream,
        CancellationToken cancellationToken = default);
}

public class Trading212CsvRecord
{
    public string Action { get; set; } = string.Empty;
    public DateTime Time { get; set; }
    public string Isin { get; set; } = string.Empty;
    public string Ticker { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Notes { get; set; } = string.Empty;
    public string Id { get; set; } = string.Empty;

    public decimal NumberOfShares { get; set; }
    public decimal PricePerShare { get; set; }
    public string CurrencyPricePerShare { get; set; } = string.Empty;
    public decimal ExchangeRate { get; set; }

    public decimal Result { get; set; }
    public string CurrencyResult { get; set; } = string.Empty;

    public decimal Total { get; set; }
    public string CurrencyTotal { get; set; } = string.Empty;

    public decimal WithholdingTax { get; set; }
    public string CurrencyWithholdingTax { get; set; } = string.Empty;

    public decimal StampDutyReserveTax { get; set; }
    public string CurrencyStampDutyReserveTax { get; set; } = string.Empty;

    public decimal CurrencyConversionFee { get; set; }
    public string CurrencyCurrencyConversionFee { get; set; } = string.Empty;

    public decimal ChargeAmount { get; set; }
    public string CurrencyChargeAmount { get; set; } = string.Empty;

    public decimal DepositFee { get; set; }
    public string CurrencyDepositFee { get; set; } = string.Empty;
}