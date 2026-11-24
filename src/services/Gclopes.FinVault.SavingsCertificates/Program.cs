using Gclopes.FinVault.SavingsCertificates.DependencyInjection;

namespace Gclopes.FinVault.SavingsCertificates;

/// <summary>
/// Main entry point for the SavingsCertificates microservice.
/// </summary>
public static class Program
{
    /// <summary>
    /// Application entry point.
    /// </summary>
    public static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Services
            .AddSavingsCertificatesSwagger()
            .AddSavingsCertificatesCors(builder.Configuration)
            .AddSavingsCertificatesAuthentication(builder.Configuration)
            .AddSavingsCertificatesPersistence(builder.Configuration)
            .AddSavingsCertificatesApplication();

        var app = builder.Build();

        app.UseSavingsCertificatesPipeline();

        await app.RunAsync();
    }
}