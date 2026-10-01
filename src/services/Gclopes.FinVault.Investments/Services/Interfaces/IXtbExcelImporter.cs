using Gclopes.FinVault.Investments.Data.Entities;

namespace Gclopes.FinVault.Investments.Services.Interfaces;

public interface IXtbExcelImporter
{
    // Reads the three sheets (Closed Positions, Cash Operations, Open Positions) of an XTB .xlsx export
    Task<XtbWorkbook> ParseAsync(Stream xlsxStream, CancellationToken cancellationToken = default);
}

public sealed record XtbWorkbook(
    IReadOnlyList<XtbClosedPosition> ClosedPositions,
    IReadOnlyList<XtbCashOperation> CashOperations,
    IReadOnlyList<XtbOpenPosition> OpenPositions);

