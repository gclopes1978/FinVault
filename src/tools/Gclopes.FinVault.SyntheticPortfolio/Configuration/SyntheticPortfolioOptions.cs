namespace Gclopes.FinVault.SyntheticPortfolio.Configuration;

/// <summary>
/// Defines configuration options for synthetic portfolio generation.
/// </summary>
public sealed class SyntheticPortfolioOptions
{
    public const string SectionName = "SyntheticPortfolio";

    public string OllamaBaseUrl { get; set; } = "http://localhost:11434";

    public string OllamaModel { get; set; } = "qwen3:4b";

    public string InputDirectory { get; set; } = "data/input";

    public string OutputDirectory { get; set; } = "data/output";

    public int HistoryMonths { get; set; } = 24;

    public string Culture { get; set; } = "pt-PT";

    public string PythonExecutable { get; set; } = "src/services/Gclopes.FinVault.PdfParser/.venv/bin/python";

    public string PdfParserScriptPath { get; set; } = "src/services/Gclopes.FinVault.PdfParser/synthetic_parser.py";

}