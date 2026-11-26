using System.Diagnostics;
using System.Text.Json;
using Gclopes.FinVault.SyntheticPortfolio.Abstractions;
using Gclopes.FinVault.SyntheticPortfolio.Configuration;
using Microsoft.Extensions.Options;

namespace Gclopes.FinVault.SyntheticPortfolio.Infrastructure.PdfExtraction;

/// <summary>
/// Executes the isolated Python PDF extraction process.
/// </summary>
public sealed class PythonPdfExtractionService(
    IOptions<SyntheticPortfolioOptions> options) : IPdfExtractionService
{
    private readonly SyntheticPortfolioOptions _options = options.Value;

    /// <inheritdoc />
    public async Task<PdfExtractionResult> ExtractAsync(
        string filePath,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(filePath);

        var absoluteFilePath = Path.GetFullPath(filePath);
        var scriptPath = Path.GetFullPath(_options.PdfParserScriptPath);
        var pythonExecutable = Path.GetFullPath(_options.PythonExecutable);

        if (!File.Exists(absoluteFilePath))
        {
            throw new FileNotFoundException(
                "The PDF document was not found.",
                absoluteFilePath);
        }

        if (!File.Exists(scriptPath))
        {
            throw new FileNotFoundException(
                "The Python PDF parser script was not found.",
                scriptPath);
        }

        if (!File.Exists(pythonExecutable))
        {
            throw new FileNotFoundException(
                "The configured Python executable was not found.",
                pythonExecutable);
        }

        var startInfo = new ProcessStartInfo
        {
            FileName = pythonExecutable,
            WorkingDirectory = Path.GetDirectoryName(scriptPath)!,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        startInfo.ArgumentList.Add(scriptPath);
        startInfo.ArgumentList.Add(absoluteFilePath);

        using var process = new Process
        {
            StartInfo = startInfo
        };

        if (!process.Start())
        {
            throw new InvalidOperationException(
                "Failed to start the Python PDF extraction process.");
        }

        using var cancellationRegistration = cancellationToken.Register(
            () =>
            {
                try
                {
                    if (!process.HasExited)
                    {
                        process.Kill(entireProcessTree: true);
                    }
                }
                catch (InvalidOperationException)
                {
                    // The process may have exited concurrently.
                }
            });

        var outputTask = process.StandardOutput.ReadToEndAsync(
            cancellationToken);

        var errorTask = process.StandardError.ReadToEndAsync(
            cancellationToken);

        await process.WaitForExitAsync(cancellationToken);

        var output = await outputTask;
        var error = await errorTask;

        if (process.ExitCode != 0)
        {
            throw new InvalidOperationException(
                $"PDF extraction failed: {error.Trim()}");
        }

        using var document = JsonDocument.Parse(output);
        var root = document.RootElement;

        if (!root.TryGetProperty("documentType", out var documentType) ||
            !root.TryGetProperty("data", out var data))
        {
            throw new InvalidOperationException(
                "The Python PDF parser returned an invalid JSON response.");
        }

        return new PdfExtractionResult(
            documentType.GetString()
                ?? throw new InvalidOperationException(
                    "The document type is missing."),
            data.GetRawText());
    }
}
