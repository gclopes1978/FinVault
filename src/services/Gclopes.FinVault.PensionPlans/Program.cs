using Gclopes.FinVault.PensionPlans.Data;
using Gclopes.FinVault.PensionPlans.Endpoints;
using Gclopes.FinVault.PensionPlans.Middlewares;
using Gclopes.FinVault.PensionPlans.Services.Implementations;
using Gclopes.FinVault.PensionPlans.Services.Interfaces;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using System.Text;

namespace Gclopes.FinVault.PensionPlans;

public static class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Configure OpenAPI/Swagger documentation
        builder.Services.AddControllers();
        builder.Services.AddEndpointsApiExplorer();

        builder.Services.AddSwaggerGen(options =>
        {
            options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Name = "Authorization",
                Type = SecuritySchemeType.Http,
                Scheme = "bearer",
                BearerFormat = "JWT",
                In = ParameterLocation.Header,
                Description = "Enter your JWT token."
            });

            options.AddSecurityRequirement(document =>
                new OpenApiSecurityRequirement
                {
                    [new OpenApiSecuritySchemeReference("Bearer", document)] = []
                });
        });
        // Register the custom global runtime crash exception handler service into the IoC injection infrastructure
        builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

        var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

        // Configure the Application Database Context with SQL Server
        builder.Services.AddDbContext<ApplicationDbContext>(options =>
        {
            options.UseSqlServer(connectionString);
            options.ConfigureWarnings(w => w.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.RelationalEventId.PendingModelChangesWarning));
        });

        // Register the financial movement service interfaces and implementations
        // Scoped lifetime matches ApplicationDbContext's default lifetime, so each HTTP
        // request gets its own service instance backed by its own DbContext instance.
        builder.Services.AddScoped<IPensionInsuranceService, PensionInsuranceService>();
        builder.Services.AddScoped<IPensionFundService, PensionFundService>();

        builder.Services.AddLogging();

        builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = builder.Configuration["JwtSettings:Issuer"],
                    ValidAudience = builder.Configuration["JwtSettings:Audience"],
                    IssuerSigningKey = new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(builder.Configuration["JwtSettings:Secret"] ??
                        throw new InvalidOperationException("JwtSettings:Secret is missing!")))
                };
                options.Events = new JwtBearerEvents
                {
                    OnAuthenticationFailed = context =>
                    {
                        // Log via the framework logger instead of Console so this shows up in structured logs / containers
                        var logger = context.HttpContext.RequestServices
                            .GetRequiredService<ILoggerFactory>()
                            .CreateLogger("JwtAuthentication");
                        logger.LogWarning(context.Exception, "JWT authentication failed.");
                        return Task.CompletedTask;
                    }
                };
            });

        builder.Services.AddAuthorization();

        var app = builder.Build();

        // Route outbound error traffic through the registered global exception handler middleware
        app.UseExceptionHandler();

        // Enable Swagger JSON and UI (currently exposed in all environments; wrap in
        // an app.Environment.IsDevelopment() check before shipping to a shared environment)
        app.UseSwagger();
        app.UseSwaggerUI();

        ApplyMigrations(app);

        app.UseHttpsRedirection();
        app.UseCors("AllowAll");
        app.UseAuthentication();
        app.UseAuthorization();

        // Configure application routing endpoints
        app.MapPensionPlansEndpoints();

        app.Run();
    }

    static void ApplyMigrations(WebApplication app)
    {
        using var scope = app.Services.CreateScope();

        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var logger = scope.ServiceProvider
            .GetRequiredService<ILoggerFactory>()
            .CreateLogger("DatabaseMigrations");

        try
        {
            logger.LogInformation("Checking database migrations...");

            context.Database.Migrate();

            logger.LogInformation("Database migrations applied successfully.");
        }
        catch (Exception ex)
        {
            logger.LogCritical(ex, "Failed to apply database migrations.");
        }
    }
}