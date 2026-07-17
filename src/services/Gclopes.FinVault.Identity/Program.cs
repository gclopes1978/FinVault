using FluentValidation;
using Gclopes.FinVault.Identity.Application.Behaviors;
using Gclopes.FinVault.Identity.Data;
using Gclopes.FinVault.Identity.Middlewares;
using Gclopes.FinVault.Identity.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;
namespace Gclopes.FinVault.Identity;

/// <summary>
/// Main entry point for the Identity Microservice.
/// Configures the dependency injection container and the HTTP request pipeline.
/// </summary>
public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // --- Configuration Setup ---
        // Binds JWT settings from appsettings.json
        var jwtSettings = builder.Configuration.GetSection("JwtSettings");
        var secretKey = jwtSettings["Secret"]!;
        var issuer = jwtSettings["Issuer"]!;
        var audience = jwtSettings["Audience"]!;

        // --- Dependency Injection ---
        // Registers the TokenService as Scoped, ensuring a new instance per request
        builder.Services.AddScoped<TokenService>();


        // --- Authentication & Authorization ---
        // Configures the Authentication services using JWT Bearer schema
        builder.Services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = issuer,
                ValidAudience = audience,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)),
                ClockSkew = TimeSpan.Zero // Remove o atraso de 5 minutos na expiração
            };
        });

        // Registers authorization and controllers in the DI container
        builder.Services.AddAuthorization();
        builder.Services.AddControllers();


        // Define the connection string (usually from appsettings.json)
        var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

        // Register the DbContext to use SQL Server (or your DB provider of choice)
        builder.Services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlServer(connectionString));

        // Register Identity using our DbContext
        builder.Services.AddIdentity<IdentityUser, IdentityRole>()
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddDefaultTokenProviders();

        builder.Services.AddValidatorsFromAssembly(typeof(Program).Assembly);

        builder.Services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(typeof(Program).Assembly);
            cfg.AddOpenBehavior(typeof(ValidationBehavior<,>));
        });

        // --- Application Pipeline ---
        // Builds the application based on the configuration above
        var app = builder.Build();

        // Adiciona isto logo após o var app = builder.Build();

        using var scope = app.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        db.Database.EnsureCreated(); // Cria a base de dados se não existir


        app.UseMiddleware<ExceptionHandlingMiddleware>();

        // Enables authentication and authorization middleware
        app.UseAuthentication();
        app.UseAuthorization();
        // Maps attribute-routed controllers to the request pipeline
        app.MapControllers();

        // Starts the web server and listens for incoming requests
        app.Run();
    }
}