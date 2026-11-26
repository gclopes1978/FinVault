namespace Gclopes.FinVault.SyntheticPortfolio.Abstractions;

/// <summary>
/// Defines the contract for interacting with a local Ollama model.
/// </summary>
public interface IOllamaClient
{
    /// <summary>
    /// Generates a structured response from the configured model.
    /// </summary>
    Task<string> GenerateJsonAsync(
        string systemPrompt,
        string userPrompt,
        CancellationToken cancellationToken = default);
}
