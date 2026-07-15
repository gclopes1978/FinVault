using Gclopes.FinVault.SavingsCertificates.Data;
using Gclopes.FinVault.SavingsCertificates.Data.Entities;
using Gclopes.FinVault.SavingsCertificates.DTO;
using Gclopes.FinVault.SavingsCertificates.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Gclopes.FinVault.SavingsCertificates.Endpoints;

public static class SavingsBondsEndpoints
{
    public static void MapSavingsBondsEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/savings-bonds")
            .WithName("SavingsBonds")
            .RequireAuthorization()
            .WithOpenApi();

        group.MapGet("/", GetAllBondsAsync)
            .WithName("GetAllBonds")
            .WithOpenApi()
            .Produces<List<SavingsBondResponseDto>>(StatusCodes.Status200OK);

        group.MapGet("/{id:guid}/valuation", GetBondValuationAsync)
            .WithName("GetBondValuation")
            .WithOpenApi()
            .Produces<object>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound);

        group.MapPost("/", CreateBondAsync)
            .WithName("CreateSavingsBond")
            .WithOpenApi()
            .Produces<SavingsBond>(StatusCodes.Status201Created);

        group.MapDelete("/{id:guid}", DeleteBondAsync)
            .WithName("DeleteSavingsBond")
            .WithOpenApi()
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status404NotFound);
    }

    private static async Task<IResult> GetAllBondsAsync(SavingsBondDbContext db, IInterestCalculationService calcService, CancellationToken ct)
    {
        var bonds = await db.SavingsBonds.ToListAsync(ct);

        var result = new List<SavingsBondResponseDto>(bonds.Count);
        foreach (var bond in bonds)
        {
            var accumulatedInterest = await calcService.CalculateCurrentValueAsync(bond, ct);
            result.Add(new SavingsBondResponseDto(
                bond.Id,
                bond.Series,
                bond.BaseRate,
                bond.SubscriptionDate,
                bond.UnitValue,
                bond.Quantity,
                accumulatedInterest
            ));
        }

        return Results.Ok(result);
    }

    private static async Task<IResult> GetBondValuationAsync(
        Guid id,
        SavingsBondDbContext db,
        IInterestCalculationService calcService,
        CancellationToken ct)
    {
        var bond = await db.SavingsBonds.FindAsync([id], ct);
        if (bond is null) return Results.NotFound(new { message = "Bond not found" });

        var currentValue = await calcService.CalculateCurrentValueAsync(bond, ct);

        return Results.Ok(new
        {
            bond.Id,
            bond.Series,
            CurrentValue = currentValue,
            CalculationDate = DateTime.UtcNow
        });
    }

    private static async Task<IResult> CreateBondAsync(SavingsBondCreateDto dto, SavingsBondDbContext db, CancellationToken ct)
    {
        // Map the DTO to the entity
        var bond = new SavingsBond
        {
            Id = Guid.NewGuid(), // Generate the ID here
            Series = dto.Series,
            BaseRate = dto.BaseRate,
            SubscriptionDate = dto.SubscriptionDate,
            UnitValue = dto.UnitValue,
            Quantity = dto.Quantity
        };

        db.SavingsBonds.Add(bond);
        await db.SaveChangesAsync(ct);

        return Results.Created($"/api/savings-bonds/{bond.Id}", bond);
    }

    private static async Task<IResult> DeleteBondAsync(Guid id, SavingsBondDbContext db, CancellationToken ct)
    {
        var bond = await db.SavingsBonds.FindAsync([id], ct);
        if (bond is null) return Results.NotFound();

        db.SavingsBonds.Remove(bond);
        await db.SaveChangesAsync(ct);
        return Results.NoContent();
    }
}
