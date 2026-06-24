using Microsoft.EntityFrameworkCore;
using Gclopes.FinVault.CurrentAccount.Data;
using Gclopes.FinVault.CurrentAccount.Endpoints;
using Gclopes.FinVault.CurrentAccount.Services.Interfaces;
using Gclopes.FinVault.CurrentAccount.Services.Implementations;

namespace Gclopes.FinVault.CurrentAccount;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Configure OpenAPI/Swagger documentation
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddOpenApi();

        // Configure the Application Database Context with SQL Server
        builder.Services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

        // Register the financial movement service interfaces and implementations
        builder.Services.AddScoped<IMovementService, MovementService>();

        var app = builder.Build();

        // Enable OpenAPI documentation mapping for all environments
        app.MapOpenApi();

        // Configure application routing endpoints
        app.MapMovementEndpoints();

        app.Run();
    }
}