using Gclopes.FinVault.PensionPlans.DependencyInjection;

namespace Gclopes.FinVault.PensionPlans;

/// <summary>
/// Main entry point for the PensionPlans microservice.
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
            .AddPensionPlansSwagger()
            .AddPensionPlansCors(builder.Configuration)
            .AddPensionPlansAuthentication(builder.Configuration)
            .AddPensionPlansPersistence(builder.Configuration)
            .AddPensionPlansApplication();

        var app = builder.Build();

        app.UsePensionPlansPipeline();

        app.Run();
    }
}