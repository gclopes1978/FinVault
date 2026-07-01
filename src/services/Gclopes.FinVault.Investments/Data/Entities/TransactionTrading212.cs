namespace Gclopes.FinVault.Investments.Data.Entities;

public class TransactionTrading212
{
    public int Id { get; set; } // Chave Primária gerada pela BD
    public string T212Id { get; set; } = string.Empty; // O ID que vem do CSV da T212

    public string Action { get; set; } = string.Empty; // Market buy, Market sell, Dividend, etc.
    public DateTime Time { get; set; }
    public string Isin { get; set; } = string.Empty;
    public string Ticker { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Notes { get; set; } // Pode ser vazio
    public decimal NumberOfShares { get; set; }
    public decimal PricePerShare { get; set; }
    public string CurrencyPricePerShare { get; set; } = string.Empty;
    public decimal ExchangeRate { get; set; }
    public decimal Total { get; set; }
    public string CurrencyTotal { get; set; } = string.Empty;
    public decimal WithholdingTax { get; set; }
    public string CurrencyWithholdingTax { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; }
}