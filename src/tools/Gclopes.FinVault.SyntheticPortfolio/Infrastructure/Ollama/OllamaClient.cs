using System.Net.Http.Json;
using System.Text.Json;
using Gclopes.FinVault.SyntheticPortfolio.Abstractions;
using Gclopes.FinVault.SyntheticPortfolio.Configuration;
using Microsoft.Extensions.Options;

namespace Gclopes.FinVault.SyntheticPortfolio.Infrastructure.Ollama;

/// <summary>
/// Provides HTTP communication with a local Ollama instance.
/// </summary>
public sealed class OllamaClient(
    HttpClient httpClient,
    IOptions<SyntheticPortfolioOptions> options) : IOllamaClient
{
    private readonly SyntheticPortfolioOptions _options = options.Value;

    /// <inheritdoc />
    public async Task<string> GenerateJsonAsync(
        string systemPrompt,
        string userPrompt,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(systemPrompt);
        ArgumentException.ThrowIfNullOrWhiteSpace(userPrompt);

        var request = new
        {
            model = _options.OllamaModel,
            messages = new[]
            {
                new { role = "system", content = systemPrompt },
                new { role = "user", content = userPrompt }
            },
            format = "json",
            stream = false
        };

        using var response = await httpClient.PostAsJsonAsync(
            "api/chat",
            request,
            cancellationToken);

        response.EnsureSuccessStatusCode();

        await using var responseStream =
            await response.Content.ReadAsStreamAsync(cancellationToken);

        using var document = await JsonDocument.ParseAsync(
            responseStream,
            cancellationToken: cancellationToken);

        if (!document.RootElement.TryGetProperty("message", out var message) ||
            !message.TryGetProperty("content", out var content))
        {
            throw new InvalidOperationException(
                "Ollama returned an invalid response.");
        }

        var result = content.GetString();

        if (string.IsNullOrWhiteSpace(result))
        {
            throw new InvalidOperationException(
                "Ollama returned an empty response.");
        }

        return result;
    }
}
