using Gclopes.FinVault.Investments.Extensions.DependencyInjection;

namespace Gclopes.FinVault.Investments;

/// <summary>
/// Main entry point for the Investments microservice.
/// </summary>
public class Program
{
    protected Program()
    {
        
    }
    /// <summary>
    /// Application entry point.
    /// </summary>
    public static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Services
            .AddInvestmentsCors(builder.Configuration)
            .AddInvestmentsSwagger()
            .AddInvestmentsApplication()
            .AddInvestmentsPersistence(builder.Configuration)
            .AddInvestmentsAuthentication(builder.Configuration);

        var app = builder.Build();

        app.UseInvestmentsPipeline();

        await app.RunAsync();
    }
}