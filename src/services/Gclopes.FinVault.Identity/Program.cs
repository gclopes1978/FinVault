using Gclopes.FinVault.Identity.DependencyInjection;

namespace Gclopes.FinVault.Identity;

/// <summary>
/// Main entry point for the Identity Microservice.
/// </summary>
public partial class Program
{
    protected Program()
    {
    }

    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Services
            .AddFinVaultPersistence(builder.Configuration)
            .AddFinVaultIdentity()
            .AddFinVaultAuthentication(builder.Configuration)
            .AddFinVaultApplication();

        var app = builder.Build();

        app.UseFinVaultPipeline();

        app.Run();
    }
}