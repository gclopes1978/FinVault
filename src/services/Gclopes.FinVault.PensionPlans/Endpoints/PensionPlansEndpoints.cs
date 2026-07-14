using Gclopes.FinVault.PensionPlans.Endpoints.DTO;
using Gclopes.FinVault.PensionPlans.Services.Interfaces;

namespace Gclopes.FinVault.PensionPlans.Endpoints;

public static class PensionPlansEndpoints
{
    public static void MapPensionPlansEndpoints(this IEndpointRouteBuilder app)
    {
        // GET is intended for authenticated end users (e.g. the Angular frontend showing
        // the pension plan statement to the logged-in owner), so it stays behind the JWT
        // bearer authentication configured in Program.cs.
        var movements = app.MapGroup("/api/pension-plans/movements")
                   .WithOpenApi();

        static async Task<IResult> GetAllMovements<T>(Func<Task<IEnumerable<T>>> serviceCall)
            => Results.Ok(await serviceCall());

        movements.MapGet("/funds", (IPensionFundService s) => GetAllMovements(s.GetAllMovementsAsync))
             .WithName("GetPensionFundMovements")
             .WithTags("PensionFundMovements")
             .RequireAuthorization();

        movements.MapGet("/insurance", (IPensionInsuranceService s) => GetAllMovements(s.GetAllMovementsAsync))
             .WithName("GetPensionInsuranceMovements")
             .WithTags("PensionInsuranceMovements")
             .RequireAuthorization();

        movements.MapPost("/funds", ImportFundMovementsAsync)
                 .WithName("ImportPensionFundMovements")
                 .WithTags("PensionFundMovements");

        movements.MapPost("/insurance", ImportInsuranceMovementsAsync)
                 .WithName("ImportPensionInsuranceMovements")
                 .WithTags("PensionInsuranceMovements");
    }

    private static async Task<IResult> ImportFundMovementsAsync(
        PensionFundMovementDto movement,
        IPensionFundService movementService)
    {
        
        try
        {
            var result = await movementService.ImportFundMovementsAsync(movement);
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

    // Handles POST /api/pension-plans/movements
    // Binds the request body directly as a List<PensionInsuranceMovementDto> because the PDF
    // parser posts a raw JSON array (no wrapping envelope) - see PensionInsuranceMovementDto.cs.
    private static async Task<IResult> ImportInsuranceMovementsAsync(
        List<PensionInsuranceMovementDto> movements,
        IPensionInsuranceService movementService)
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
