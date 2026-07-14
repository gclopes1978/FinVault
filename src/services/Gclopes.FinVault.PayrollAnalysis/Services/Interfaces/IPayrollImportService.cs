using Gclopes.FinVault.PayrollAnalysis.Models;

namespace Gclopes.FinVault.PayrollAnalysis.Services.Interfaces;

public interface IPayrollImportService
{
    // Persists a parsed payslip. Idempotent on SourceDocumentId: re-importing the same document
    // returns the existing record instead of creating a duplicate.
    Task<PayrollImportResponse> ImportAsync(ImportPayrollRequest request);

    // Retrieves all imported payslips, most recent first.
    Task<IEnumerable<PayrollImportResponse>> GetAllImportsAsync();

    // Retrieves a specific payslip import by its source document identifier.
    Task<PayrollImportResponse?> GetImportBySourceDocumentIdAsync(Guid sourceDocumentId);
}
