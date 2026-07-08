using Gclopes.FinVault.PensionPlans.Data;
using Gclopes.FinVault.PensionPlans.Endpoints;
using Gclopes.FinVault.PensionPlans.Middlewares;
using Gclopes.FinVault.PensionPlans.Services.Implementations;
using Gclopes.FinVault.PensionPlans.Services.Interfaces;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;

namespace Gclopes.FinVault.PensionPlans;
public class Program
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
                Scheme = "Bearer",
                BearerFormat = "JWT",
                In = ParameterLocation.Header,
                Description = "Enter 'Bearer' [space] and then your token in the text input below.\r\n\r\nExample: \"Bearer 12345abcdef\""
            });

            // Require the Bearer scheme on protected endpoints so Swagger UI attaches the token automatically
            options.AddSecurityRequirement(new OpenApiSecurityRequirement
                                            {
                                                {
                                                    new OpenApiSecurityScheme
                                                    {
                                                        Reference = new OpenApiReference
                                                        {
                                                            Type = ReferenceType.SecurityScheme,
                                                            Id = "Bearer"
                                                        }
                                                    },
                                                    Array.Empty<string>()
                                                }
                                            });
        });

        // CORS policy used by app.UseCors("AllowAll") below.
        // TODO: restrict origins before deploying to any shared/cloud environment.
        builder.Services.AddCors(options =>
        {
            options.AddPolicy("AllowAll", policy =>
            {
                policy.AllowAnyOrigin()
                      .AllowAnyMethod()
                      .AllowAnyHeader();
            });
        });

        // Inject the required standard baseline ProblemDetails schema generating dependencies into app runtime
        builder.Services.AddProblemDetails();

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

        app.MapOpenApi();
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
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();

        try
        {
            // Instead of Migrate(), which relies on the history table,
            // use EnsureCreated() to map the current DbContext model to the database.
            // If tables exist, it does nothing; if they don't, it creates them.
            if (context.Database.EnsureCreated())
            {
                logger.LogInformation("Database created successfully.");
            }
            else
            {
                logger.LogInformation("Database already exists. Schema verification complete.");
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while synchronizing the database schema.");
        }
    }
}