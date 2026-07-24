using FluentValidation;
using Gclopes.FinVault.CurrentAccount.Endpoints.DTO;
using Gclopes.FinVault.CurrentAccount.Models;
using Gclopes.FinVault.CurrentAccount.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace Gclopes.FinVault.CurrentAccount.Endpoints;

public static class MovementEndpoints
{
    public static void MapMovementEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/movements")
                       .WithTags("Movements");

        // Dentro do teu MapMovementEndpoints...
        group.MapPatch("/{id}/note", UpdateMovementNoteAsync)
             .WithName("UpdateMovementNote")
             .RequireAuthorization()
             .WithOpenApi();

        // GET /api/movements - Retrieve a page of financial movements, optionally filtered by
        // TransactionDate/ValueDate range, sorted TransactionDate desc, ValueDate desc by default
        group.MapGet("/", GetMovementsAsync)
             .WithName("GetMovements")
             .RequireAuthorization()
             .WithOpenApi();

        // GET /api/movements/totals - Aggregate credit/debit totals (same date filters) plus the
        // account's current balance, so the frontend doesn't need to load every movement to show them
        group.MapGet("/totals", GetMovementTotalsAsync)
             .WithName("GetMovementTotals")
             .RequireAuthorization()
             .WithOpenApi();

        group.MapPost("/", async (BankStatementPayloadDto request, IMovementService service, ClaimsPrincipal user) =>
        {
            try
            {
                var userId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "system";
                var result = await service.RegisterBankStatementAsync(request, userId);
                return Results.Ok(result);
            }
            catch (InvalidOperationException ex)
            {
                // Erros de negócio (como matemática errada)
                return Results.BadRequest(new BankStatementResponse(Guid.Empty, ex.Message, DateTime.UtcNow));
            }
            catch (Exception)
            {
                // Erros inesperados
                return Results.Problem("An unexpected error occurred.");
            }
        })
        .WithName("RegisterStatement")
        .WithOpenApi();
    }

    // Handles the retrieval of a page of recorded movements, filtered by date range
    private static async Task<IResult> GetMovementsAsync(
        [AsParameters] MovementQueryParameters query,
        IMovementService movementService,
        ClaimsPrincipal user)
    {
        // Extract UserId from token claims
        var userId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value
                  ?? user.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;
        if (string.IsNullOrEmpty(userId)) return Results.Unauthorized();

        var result = await movementService.GetMovementsAsync(query);
        return Results.Ok(result);
    }

    // Handles the retrieval of credit/debit totals plus the current account balance
    private static async Task<IResult> GetMovementTotalsAsync(
        [AsParameters] MovementQueryParameters query,
        IMovementService movementService,
        ClaimsPrincipal user)
    {
        var userId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value
                  ?? user.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;
        if (string.IsNullOrEmpty(userId)) return Results.Unauthorized();

        var result = await movementService.GetMovementTotalsAsync(query);
        return Results.Ok(result);
    }

    private static async Task<IResult> UpdateMovementNoteAsync(
        Guid id,
        [FromBody] UpdateNoteRequest request,
        IMovementService movementService,
        ClaimsPrincipal user)
    {
        var userId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value
                  ?? user.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;
        if (string.IsNullOrEmpty(userId)) return Results.Unauthorized();

        if (request?.Note is null) return Results.BadRequest("Note is required.");

        // Chamamos o serviço para atualizar; devolvemos o movimento atualizado para que o
        // frontend possa confirmar o novo estado sem depender apenas da mutação local
        var updated = await movementService.UpdateNoteAsync(id, request.Note);

        return updated is not null ? Results.Ok(updated) : Results.NotFound();
    }
}