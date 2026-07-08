using Gclopes.FinVault.PayrollAnalysis.Data;
using Gclopes.FinVault.PayrollAnalysis.Models;
using Gclopes.FinVault.PayrollAnalysis.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Gclopes.FinVault.PayrollAnalysis.Services.Implementations;

public class PayrollImportService(PayrollAnalysisDbContext context) : IPayrollImportService
{
    private readonly PayrollAnalysisDbContext _context = context;

    public async Task<PayrollImportResponse> ImportAsync(ImportPayrollRequest request)
    {
        // Idempotency guard: the PdfParser pipeline can redeliver the same document (RabbitMQ
        // at-least-once semantics) or the user can re-upload the same payslip by mistake.
        var existing = await _context.PayrollImports
            .Include(i => i.Items)
            .FirstOrDefaultAsync(i => i.SourceDocumentId == request.SourceDocumentId);

        if (existing is not null)
        {
            return MapToResponse(existing);
        }

        var import = new PayrollImport
        {
            Id = Guid.NewGuid(),
            SourceDocumentId = request.SourceDocumentId,
            ImportedAt = DateTime.UtcNow,
            Items = request.Data.Select(item => new PayrollItem
            {
                Id = Guid.NewGuid(),
                Code = item.Code,
                Field = item.Field,
                Description = item.Description,
                ReferenceMonth = item.Date,
                Quantity = item.Quantity,
                Rate = item.Rate,
                UnitValue = item.UnitValue,
                Amount = item.Amount,
                Type = item.Type
            }).ToList()
        };

        _context.PayrollImports.Add(import);
        await _context.SaveChangesAsync();

        return MapToResponse(import);
    }

    public async Task<IEnumerable<PayrollImportResponse>> GetAllImportsAsync()
    {
        var imports = await _context.PayrollImports
            .Include(i => i.Items)
            .OrderByDescending(i => i.ImportedAt)
            .ToListAsync();

        return imports.Select(MapToResponse);
    }

    public async Task<PayrollImportResponse?> GetImportBySourceDocumentIdAsync(Guid sourceDocumentId)
    {
        var import = await _context.PayrollImports
            .Include(i => i.Items)
            .FirstOrDefaultAsync(i => i.SourceDocumentId == sourceDocumentId);

        return import is null ? null : MapToResponse(import);
    }

    private static PayrollImportResponse MapToResponse(PayrollImport import) =>
        new(
            import.Id,
            import.SourceDocumentId,
            import.ImportedAt,
            import.Items.Select(item => new PayrollItemResponse(
                item.Id,
                item.Code,
                item.Field,
                item.Description,
                item.ReferenceMonth,
                item.Quantity,
                item.Rate,
                item.UnitValue,
                item.Amount,
                item.Type
            )).ToList()
        );
}
