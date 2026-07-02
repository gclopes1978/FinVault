namespace Gclopes.FinVault.DocumentIngestion.Models;

public record DocumentUploadedEvent(string FilePath, Guid UserId, DateTime UploadedAt);