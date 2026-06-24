namespace Gclopes.FinVault.Gateway;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Configure CORS for the Angular frontend application
        builder.Services.AddCors(options =>
        {
            options.AddPolicy("AngularFrontendPolicy", policy =>
            {
                policy.WithOrigins("http://localhost:4200")
                      .AllowAnyMethod()
                      .AllowAnyHeader();
            });
        });

        // Initialize YARP Reverse Proxy using configuration from appsettings.json
        builder.Services.AddReverseProxy()
            .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

        var app = builder.Build();

        // Enable routing and apply the configured CORS middleware globally
        app.UseRouting();
        app.UseCors();

        // Map the reverse proxy endpoints into the request pipeline
        app.MapReverseProxy();

        app.Run();
    }
}