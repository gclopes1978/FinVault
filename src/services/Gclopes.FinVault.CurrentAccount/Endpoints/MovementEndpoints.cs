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
            .RequireAuthorization();

        group.MapGet("/", GetMovementsAsync)
            .WithName("GetMovements")
            .RequireAuthorization();

        group.MapGet("/years", GetAvailableYearsAsync)
            .WithName("GetAvailableMovementYears")
            .RequireAuthorization();

        group.MapGet("/totals/year", GetTotalsByMonthAndYearAsync)
             .WithName("GetTotalsByMonthAndYear")
             .RequireAuthorization();

        group.MapGet("/totals", GetMovementTotalsAsync)
            .WithName("GetMovementTotals")
            .RequireAuthorization();

        group.MapPost("/", RegisterStatementAsync)
            .WithName("RegisterStatement")
            .RequireAuthorization();
    }

    private static async Task<IResult> GetAvailableYearsAsync(IMovementService movementService)
    {
        List<int> years = await movementService.GetAvailableYearsAsync();

        return Results.Ok(years);
    }

    private static async Task<IResult> GetTotalsByMonthAndYearAsync(
        int year,
        IMovementService movementService,
        ILoggerFactory loggerFactory)
    {
        var logger = loggerFactory.CreateLogger("MovementEndpoints");

        if (logger.IsEnabled(LogLevel.Information))
        {
            logger.LogInformation("Getting totals for year {Year}.", year);
        }

        var result = await movementService.GetTotalsByYearMonthResponse(year);

        return Results.Ok(result);
    }


    private static async Task<IResult> GetMovementsAsync(
        [AsParameters] MovementQueryParameters query,
        IMovementService movementService,
        ILoggerFactory loggerFactory)
    {
        var logger = loggerFactory.CreateLogger("MovementEndpoints");
        logger.LogInformation("Getting movements.");

        

        var result = await movementService.GetMovementsAsync(query);
        if (logger.IsEnabled(LogLevel.Debug))
        {
            logger.LogDebug("Retrieved {Count} movements.", result.Items?.Count ?? 0);
        }
        return Results.Ok(result);
    }

    private static async Task<IResult> GetMovementTotalsAsync(
        [AsParameters] MovementQueryParameters query,
        IMovementService movementService,
        ILoggerFactory loggerFactory)
    {
        var logger = loggerFactory.CreateLogger("MovementEndpoints");
        logger.LogInformation("Getting movement totals.");


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