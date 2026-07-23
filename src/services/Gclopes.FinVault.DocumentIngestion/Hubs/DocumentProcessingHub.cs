using Microsoft.AspNetCore.SignalR;

namespace Gclopes.FinVault.DocumentIngestion.Hubs;

// Pure server -> client push channel: broadcasts a "documentProcessed" event to every connected
// PDF Upload Centre client whenever the PdfParser reports a document as finished (success or
// error), so the import history updates live instead of the frontend having to poll for it.
// No client-invokable methods are needed yet.
public class DocumentProcessingHub : Hub
{
}
