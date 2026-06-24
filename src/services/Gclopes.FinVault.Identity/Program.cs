namespace Gclopes.FinVault.Identity;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Add basic logging configuration to the console and debug output
        builder.Logging.ClearProviders();
        builder.Logging.AddConsole();
        builder.Logging.AddDebug();

        // Add foundational services for health checking capabilities
        builder.Services.AddHealthChecks();

        var app = builder.Build();

        // Configure the HTTP request pipeline for development environments
        if (app.Environment.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
        }

        // Map basic health check endpoint for microservice monitoring
        app.MapHealthChecks("/health");

        // Basic root endpoint for immediate deployment verification
        app.MapGet("/", () => "FinVault Identity Microservice Skeleton is running.");

        app.Run();
    }
}