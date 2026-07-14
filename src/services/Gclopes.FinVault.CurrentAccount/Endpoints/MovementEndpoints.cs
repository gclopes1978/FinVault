using FluentValidation;
using Gclopes.FinVault.CurrentAccount.Endpoints.DTO;
using Gclopes.FinVault.CurrentAccount.Models;
using Gclopes.FinVault.CurrentAccount.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Gclopes.FinVault.CurrentAccount.Endpoints;

public static class MovementEndpoints
{
    public static void MapMovementEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/movements")
                       .WithTags("Movements")
                       .RequireAuthorization();

        // Dentro do teu MapMovementEndpoints...
        group.MapPatch("/{id}/note", UpdateMovementNoteAsync)
             .WithName("UpdateMovementNote")
             .RequireAuthorization()
             .WithOpenApi();

        // GET /api/movements - Retrieve all financial movements using the service layer
        group.MapGet("/", GetMovementsAsync)
             .WithName("GetMovements")
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

    // Handles the retrieval of all recorded movements
    private static async Task<IResult> GetMovementsAsync(
        IMovementService movementService,
        ClaimsPrincipal user)
    {
        // Extract UserId from token claims
        var userId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId)) return Results.Unauthorized();

        var result = await movementService.GetAllMovementsAsync();
        return Results.Ok(result);
    }

    

    private static async Task<IResult> UpdateMovementNoteAsync(
        Guid id,
        [FromBody] UpdateNoteRequest request,
        IMovementService movementService,
        ClaimsPrincipal user)
    {
        var userId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId)) return Results.Unauthorized();

        // Chamamos o serviço para atualizar
        var success = await movementService.UpdateNoteAsync(id, request.Note);

        return success ? Results.NoContent() : Results.NotFound();
    }
}