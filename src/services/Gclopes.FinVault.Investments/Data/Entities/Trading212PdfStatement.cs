namespace Gclopes.FinVault.Investments.Data.Entities;

public class Trading212PdfStatement
{
    public int Id { get; set; }
    public string SourceDocumentId { get; set; } = string.Empty;
    public string DataJson { get; set; } = string.Empty;
    public DateTime ImportedAtUtc { get; set; }
}
