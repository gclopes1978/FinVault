namespace Gclopes.FinVault.PayrollAnalysis.Data;

public class PayrollItem
{
    public Guid Id { get; set; }

    public Guid PayrollImportId { get; set; }
    public PayrollImport PayrollImport { get; set; } = null!;

    // Rubric code as printed on the payslip (e.g. 10000, 20210). Distinguishes the two known
    // payslip layout versions used by the PdfParser (>= 10000 vs shorter codes).
    public int Code { get; set; }

    // Canonical field name derived from the rubric code (e.g. "Vencimento_Base").
    public string Field { get; set; } = string.Empty;

    public string? Description { get; set; }

    // Reference period printed on some rubric lines, in "YYYY-MM" format. Payslips never carry a day.
    public string? ReferenceMonth { get; set; }

    public decimal? Quantity { get; set; }
    public decimal? Rate { get; set; }
    public decimal? UnitValue { get; set; }

    // The actual earning/deduction value. Positive for "abono", negative for "desconto".
    public decimal Amount { get; set; }

    // "abono" (earning) or "desconto" (deduction)
    public string Type { get; set; } = string.Empty;
}
