namespace Gclopes.FinVault.PensionPlans.Endpoints.DTO;

// Summary returned after a batch of movements is imported. "Imported" counts the rows
// that were actually persisted; "SkippedAsDuplicate" counts rows that were already in the
// database (see PensionInsuranceService.ImportMovementsAsync for the duplicate rule) -
// this matters because the same PDF statement can legitimately be re-uploaded (e.g. during
// the demo, or if the user re-imports a document by mistake) and we don't want duplicated
// financial records every time that happens.
public record ImportPensionInsuranceMovementsResponse(
    int Imported,
    int SkippedAsDuplicate,
    DateTime ProcessedAt
);
