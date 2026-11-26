using Gclopes.FinVault.SyntheticPortfolio.Abstractions;
using Gclopes.FinVault.SyntheticPortfolio.Configuration;
using Gclopes.FinVault.SyntheticPortfolio.Infrastructure.Ollama;
using Gclopes.FinVault.SyntheticPortfolio.Infrastructure.PdfExtraction;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Gclopes.FinVault.SyntheticPortfolio.Extensions;

/// <summary>
/// Provides dependency injection extensions for the synthetic portfolio application.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers synthetic portfolio configuration.
    /// </summary>
    public static IServiceCollection AddSyntheticPortfolioConfiguration(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddOptions<SyntheticPortfolioOptions>()
            .Bind(configuration.GetSection(SyntheticPortfolioOptions.SectionName))
            .Validate(
                options => Uri.TryCreate(
                    options.OllamaBaseUrl,
                    UriKind.Absolute,
                    out var uri) &&
                    (uri.Scheme == Uri.UriSchemeHttp ||
                     uri.Scheme == Uri.UriSchemeHttps),
                "OllamaBaseUrl must be a valid HTTP or HTTPS URL.")
            .Validate(
                options => !string.IsNullOrWhiteSpace(options.OllamaModel),
                "OllamaModel is required.")
            .Validate(
                options => options.HistoryMonths > 0,
                "HistoryMonths must be greater than zero.")
            .ValidateOnStart();

        return services;
    }

    /// <summary>
    /// Registers infrastructure services.
    /// </summary>
    public static IServiceCollection AddSyntheticPortfolioInfrastructure(
        this IServiceCollection services)
    {
        services.AddHttpClient<IOllamaClient, OllamaClient>(
            (serviceProvider, client) =>
            {
                var options = serviceProvider
                    .GetRequiredService<
                        Microsoft.Extensions.Options.IOptions<SyntheticPortfolioOptions>>()
                    .Value;

                client.BaseAddress = new Uri(
                    $"{options.OllamaBaseUrl.TrimEnd('/')}/");

                client.Timeout = TimeSpan.FromMinutes(2);
            });
        
        services.AddSingleton<IPdfExtractionService, PythonPdfExtractionService>();
        return services;
    }
}
