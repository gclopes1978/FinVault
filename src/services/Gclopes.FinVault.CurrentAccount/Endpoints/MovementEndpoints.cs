using FluentValidation;
using Gclopes.FinVault.CurrentAccount.Models;
using Gclopes.FinVault.CurrentAccount.Services.Interfaces;

namespace Gclopes.FinVault.CurrentAccount.Endpoints;

public static class MovementEndpoints
{
    // Extends IEndpointRouteBuilder to map all movement-related routes cleanly
    public static void MapMovementEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/movements")
                       .WithTags("Movements");

        // POST /api/movements - Register a new financial movement using the service layer
        group.MapPost("/", RegisterMovementAsync)
             .WithName("RegisterMovement")
             .WithOpenApi();

        // GET /api/movements - Retrieve all financial movements using the service layer
        group.MapGet("/", GetMovementsAsync)
             .WithName("GetMovements")
             .WithOpenApi();
    }

    // Handles the creation of a new financial movement
    private static async Task<IResult> RegisterMovementAsync(CreateMovementRequest request, IValidator<CreateMovementRequest> validator, IMovementService movementService)
    {
        // Execute the automated FluentValidation check
        var validationResult = await validator.ValidateAsync(request);

        if (!validationResult.IsValid)
        {
            // Returns a standard 400 Bad Request with a structured dictionary of errors matching the DoD
            return Results.ValidationProblem(validationResult.ToDictionary());
        }

        // If validation passes, proceed to the service layer logic
        var result = await movementService.RegisterMovementAsync(request);
        return Results.Created($"/api/movements/{result.Id}", result);
    }

    // Handles the retrieval of all recorded movements
    private static async Task<IResult> GetMovementsAsync(IMovementService movementService)
    {
        var result = await movementService.GetAllMovementsAsync();
        return Results.Ok(result);
    }
}