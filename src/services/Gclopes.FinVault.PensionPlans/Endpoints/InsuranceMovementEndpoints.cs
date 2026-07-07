using Gclopes.FinVault.PensionPlans.Endpoints.DTO;
using Gclopes.FinVault.PensionPlans.Services.Interfaces;

namespace Gclopes.FinVault.PensionPlans.Endpoints;

public static class InsuranceMovementEndpoints
{
    public static void MapInsuranceMovementEndpoints(this IEndpointRouteBuilder app)
    {
        // GET is intended for authenticated end users (e.g. the Angular frontend showing
        // the pension plan statement to the logged-in owner), so it stays behind the JWT
        // bearer authentication configured in Program.cs.
        var authenticatedGroup = app.MapGroup("/api/pension-plans/movements")
                                     .WithTags("PensionPlanMovements")
                                     .RequireAuthorization();

        authenticatedGroup.MapGet("/", GetMovementsAsync)
                           .WithName("GetPensionPlanMovements")
                           .WithOpenApi();

        // POST is called machine-to-machine by Gclopes.FinVault.PdfParser right after it
        // finishes parsing a Lusitânia Vida / pension insurance PDF (see main.py,
        // _forward_to_dotnet_service). That internal call does not carry an end-user JWT,
        // so this route is intentionally placed in its own, unauthenticated group.
        //
        // TODO (future hardening): protect this internal ingestion route with a
        // service-to-service secret/API key instead of leaving it fully open, once the
        // deployment topology (network isolation vs. public exposure) is decided.
        var ingestionGroup = app.MapGroup("/api/pension-plans/movements")
                                 .WithTags("PensionPlanMovements");

        ingestionGroup.MapPost("/", ImportMovementsAsync)
                      .WithName("ImportPensionPlanMovements")
                      .WithOpenApi();
    }

    // Handles GET /api/pension-plans/movements
    private static async Task<IResult> GetMovementsAsync(IInsuranceMovementService movementService)
    {
        var result = await movementService.GetAllMovementsAsync();
        return Results.Ok(result);
    }

    // Handles POST /api/pension-plans/movements
    // Binds the request body directly as a List<InsuranceMovementDto> because the PDF
    // parser posts a raw JSON array (no wrapping envelope) - see InsuranceMovementDto.cs.
    private static async Task<IResult> ImportMovementsAsync(
        List<InsuranceMovementDto> movements,
        IInsuranceMovementService movementService)
    {
        if (movements is null || movements.Count == 0)
        {
            return Results.BadRequest("Request body must contain at least one movement.");
        }

        try
        {
            var result = await movementService.ImportMovementsAsync(movements);
            return Results.Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            // Business/validation errors (e.g. a malformed movement) -> 400, with the
            // reason surfaced to the caller so the pipeline can log it usefully.
            return Results.BadRequest(ex.Message);
        }
        catch (Exception)
        {
            // Unexpected errors are also caught by GlobalExceptionHandler, but returning
            // a generic Problem here keeps behaviour explicit and consistent with the
            // CurrentAccount service's MovementEndpoints.
            return Results.Problem("An unexpected error occurred while importing movements.");
        }
    }
}
