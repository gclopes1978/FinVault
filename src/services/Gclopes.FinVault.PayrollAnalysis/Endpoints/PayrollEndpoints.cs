using FluentValidation;
using Gclopes.FinVault.PayrollAnalysis.Models;
using Gclopes.FinVault.PayrollAnalysis.Services.Interfaces;

namespace Gclopes.FinVault.PayrollAnalysis.Endpoints;

public static class PayrollEndpoints
{
    public static void MapPayrollEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/payroll-analysis")
                       .WithTags("PayrollAnalysis");

        // POST /api/payroll-analysis/imports - Ingests a parsed payslip.
        // Called server-to-server by the PdfParser pipeline (FOLHASALARIAL_SERVICE_URL),
        // not exposed to end users through the Gateway, so it is intentionally unauthenticated.
        group.MapPost("/imports", ImportPayrollAsync)
             .WithName("ImportPayroll")
             .WithOpenApi();

        // GET /api/payroll-analysis/imports - Lists all imported payslips.
        group.MapGet("/imports", GetAllImportsAsync)
             .WithName("GetPayrollImports")
             .WithOpenApi();

        // GET /api/payroll-analysis/imports/{sourceDocumentId} - Retrieves a specific import.
        group.MapGet("/imports/{sourceDocumentId:guid}", GetImportBySourceDocumentIdAsync)
             .WithName("GetPayrollImportBySourceDocument")
             .WithOpenApi();
    }

    private static async Task<IResult> ImportPayrollAsync(
        ImportPayrollRequest request,
        IValidator<ImportPayrollRequest> validator,
        IPayrollImportService payrollImportService)
    {
        var validationResult = await validator.ValidateAsync(request);
        if (!validationResult.IsValid)
        {
            return Results.ValidationProblem(validationResult.ToDictionary());
        }

        var result = await payrollImportService.ImportAsync(request);
        return Results.Created($"/api/payroll-analysis/imports/{result.SourceDocumentId}", result);
    }

    private static async Task<IResult> GetAllImportsAsync(IPayrollImportService payrollImportService)
    {
        var result = await payrollImportService.GetAllImportsAsync();
        return Results.Ok(result);
    }

    private static async Task<IResult> GetImportBySourceDocumentIdAsync(
        Guid sourceDocumentId,
        IPayrollImportService payrollImportService)
    {
        var result = await payrollImportService.GetImportBySourceDocumentIdAsync(sourceDocumentId);
        return result is null ? Results.NotFound() : Results.Ok(result);
    }
}
