namespace Gclopes.FinVault.DocumentIngestion.Models;

public record DocumentUploadedEvent(Guid DocumentId, string FilePath, Guid UserId, DateTime UploadedAt);
