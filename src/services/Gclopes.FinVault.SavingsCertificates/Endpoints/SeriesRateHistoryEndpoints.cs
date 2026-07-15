using Gclopes.FinVault.SavingsCertificates.Data;
using Gclopes.FinVault.SavingsCertificates.Data.Entities;
using Gclopes.FinVault.SavingsCertificates.DTO;
using Microsoft.EntityFrameworkCore;

namespace Gclopes.FinVault.SavingsCertificates.Endpoints;

// Lets the historical monthly base rates published by the IGCP be imported and
// queried, so BaseSeriesStrategy can calculate each quarter with the rate that was
// actually in force at the time instead of a single constant snapshot rate.
// Source of truth: https://www.igcp.pt (technical sheets / monthly rate notices per series).
public static class SeriesRateHistoryEndpoints
{
    public static void MapSeriesRateHistoryEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/rate-history")
            .WithName("SeriesRateHistory")
            .RequireAuthorization()
            .WithOpenApi();

        group.MapGet("/{series}", GetHistoryForSeriesAsync)
            .WithName("GetSeriesRateHistory")
            .WithOpenApi()
            .Produces<List<SeriesRateHistoryDto>>(StatusCodes.Status200OK);

        // Upserts a single month's rate. Safe to call repeatedly for the same
        // series/month (e.g. re-importing) since it updates rather than duplicates.
        group.MapPost("/", UpsertRateAsync)
            .WithName("UpsertSeriesRate")
            .WithOpenApi()
            .Produces(StatusCodes.Status204NoContent);

        // Bulk import, e.g. pasting a full year at once from the IGCP's published table.
        group.MapPost("/bulk", UpsertRatesBulkAsync)
            .WithName("UpsertSeriesRatesBulk")
            .WithOpenApi()
            .Produces(StatusCodes.Status204NoContent);
    }

    private static async Task<IResult> GetHistoryForSeriesAsync(string series, SavingsBondDbContext db, CancellationToken ct)
    {
        var history = await db.SeriesRateHistories
            .Where(r => r.Series == series)
            .OrderBy(r => r.EffectiveMonth)
            .Select(r => new SeriesRateHistoryDto(r.Series, r.EffectiveMonth, r.BaseRate))
            .ToListAsync(ct);

        return Results.Ok(history);
    }

    private static async Task<IResult> UpsertRateAsync(SeriesRateHistoryDto dto, SavingsBondDbContext db, CancellationToken ct)
    {
        await UpsertAsync(dto, db, ct);
        await db.SaveChangesAsync(ct);
        return Results.NoContent();
    }

    private static async Task<IResult> UpsertRatesBulkAsync(List<SeriesRateHistoryDto> dtos, SavingsBondDbContext db, CancellationToken ct)
    {
        foreach (var dto in dtos)
        {
            await UpsertAsync(dto, db, ct);
        }

        await db.SaveChangesAsync(ct);
        return Results.NoContent();
    }

    private static async Task UpsertAsync(SeriesRateHistoryDto dto, SavingsBondDbContext db, CancellationToken ct)
    {
        var month = new DateOnly(dto.EffectiveMonth.Year, dto.EffectiveMonth.Month, 1);

        var existing = await db.SeriesRateHistories
            .FirstOrDefaultAsync(r => r.Series == dto.Series && r.EffectiveMonth == month, ct);

        if (existing is null)
        {
            db.SeriesRateHistories.Add(new SeriesRateHistory
            {
                Series = dto.Series,
                EffectiveMonth = month,
                BaseRate = dto.BaseRate
            });
        }
        else
        {
            existing.BaseRate = dto.BaseRate;
        }
    }
}
