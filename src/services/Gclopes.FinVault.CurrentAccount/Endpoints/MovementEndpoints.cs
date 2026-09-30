using Gclopes.FinVault.CurrentAccount.Endpoints.DTO;
using Gclopes.FinVault.CurrentAccount.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Gclopes.FinVault.CurrentAccount.Endpoints;

public static class MovementEndpoints
{
    public static void MapMovementEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/movements")
            .WithTags("Movements");

        group.MapPatch("/{id}/note", UpdateMovementNoteAsync)
            .WithName("UpdateMovementNote")
            .RequireAuthorization()
            .WithOpenApi();

        group.MapGet("/", GetMovementsAsync)
            .WithName("GetMovements")
            .RequireAuthorization()
            .WithOpenApi();

        group.MapGet("/years", GetAvailableYearsAsync)
            .WithName("GetAvailableMovementYears")
            .RequireAuthorization()
            .WithOpenApi();

        group.MapGet("/totals/year", GetTotalsByMonthAndYearAsync)
             .WithName("GetTotalsByMonthAndYear")
             .RequireAuthorization()
             .WithOpenApi();

        group.MapGet("/totals", GetMovementTotalsAsync)
            .WithName("GetMovementTotals")
            .RequireAuthorization()
            .WithOpenApi();

        group.MapPost("/", RegisterStatementAsync)
            .WithName("RegisterStatement")
            .RequireAuthorization()
            .WithOpenApi();
    }

    private static async Task<IResult> GetAvailableYearsAsync(IMovementService movementService)
    {
        List<int> years = await movementService.GetAvailableYearsAsync();

        return Results.Ok(years);
    }

    public sealed class MovementEndpointsLog;

    private static async Task<IResult> GetTotalsByMonthAndYearAsync(
        int year,
        IMovementService movementService,
        ILogger<MovementEndpointsLog> logger)
    {
        logger.LogInformation("GetTotalsByMonthAndYearAsync --> {year}", year);
        var result = await movementService.GetTotalsByYearMonthResponse(year);

        return Results.Ok(result);
    }


    private static async Task<IResult> GetMovementsAsync(
        [AsParameters] MovementQueryParameters query,
        IMovementService movementService,
        ILogger<MovementEndpointsLog> logger)
    {
        logger.LogInformation("Getting movements.");

        logger.LogDebug(
            "Movement query: {@Query}",
            query);

        var result = await movementService.GetMovementsAsync(query);

        logger.LogDebug(
            "Retrieved {Count} movements.",
            result.Items?.Count ?? 0);

        return Results.Ok(result);
    }

    private static async Task<IResult> GetMovementTotalsAsync(
        [AsParameters] MovementQueryParameters query,
        IMovementService movementService,
        ILogger<MovementEndpointsLog> logger)
    {
        logger.LogInformation("Getting movement totals.");

        logger.LogDebug(
            "Movement totals query: {@Query}",
            query);

        var result = await movementService.GetMovementTotalsAsync(query);

        return Results.Ok(result);
    }

    private static async Task<IResult> UpdateMovementNoteAsync(
        Guid id,
        [FromBody] UpdateNoteRequest request,
        IMovementService movementService)
    {
        if (string.IsNullOrWhiteSpace(request.Note))
        {
            return Results.BadRequest("Note is required.");
        }

        var updated = await movementService.UpdateNoteAsync(
            id,
            request.Note);

        return updated is not null
            ? Results.Ok(updated)
            : Results.NotFound();
    }

    private static async Task<IResult> RegisterStatementAsync(
        BankStatementPayloadDto request,
        IMovementService movementService)
    {
        try
        {
            var result = await movementService.RegisterBankStatementAsync(request);

            return Results.Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return Results.BadRequest(
                new BankStatementResponse(
                    Guid.Empty,
                    ex.Message,
                    DateTime.UtcNow));
        }
        catch (Exception)
        {
            return Results.Problem(
                "An unexpected error occurred.");
        }
    }
}