using Gclopes.FinVault.CurrentAccount.DependencyInjection;

namespace Gclopes.FinVault.CurrentAccount;

/// <summary>
/// Main entry point for the CurrentAccount microservice.
/// </summary>
public static class Program
{
    /// <summary>
    /// Application entry point.
    /// </summary>
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Services
            .AddCurrentAccountCors(builder.Configuration)
            .AddCurrentAccountSwagger()
            .AddCurrentAccountAuthentication(builder.Configuration)
            .AddCurrentAccountPersistence(builder.Configuration)
            .AddCurrentAccountApplication();

        var app = builder.Build();

        app.UseCurrentAccountPipeline();

        app.Run();
    }
}