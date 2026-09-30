using FluentValidation;
using Gclopes.FinVault.CurrentAccount.Data;
using Gclopes.FinVault.CurrentAccount.Data.Interceptors;
using Gclopes.FinVault.CurrentAccount.Endpoints;
using Gclopes.FinVault.CurrentAccount.Middlewares;
using Gclopes.FinVault.CurrentAccount.Services.Implementations;
using Gclopes.FinVault.CurrentAccount.Services.Interfaces;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;

namespace Gclopes.FinVault.CurrentAccount;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        var isDevelopment = builder.Environment.IsDevelopment();

        // Swagger (Swashbuckle only; the built-in .NET 9 OpenAPI is not used)
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen(options =>
        {
            // With Type = Http and Scheme = Bearer, Swagger UI adds the "Bearer " prefix itself
            options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Name = "Authorization",
                Type = SecuritySchemeType.Http,
                Scheme = "Bearer",
                BearerFormat = "JWT",
                In = ParameterLocation.Header,
                Description = "Enter only the JWT token (without the 'Bearer' prefix)."
            });

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

        // CORS: open only in Development, explicit origins from configuration elsewhere
        var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [];
        builder.Services.AddCors(options =>
        {
            options.AddPolicy("Default", policy =>
            {
                if (isDevelopment)
                {
                    policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader();
                }
                else
                {
                    policy.WithOrigins(allowedOrigins).AllowAnyMethod().AllowAnyHeader();
                }
            });
        });

        builder.Services.AddProblemDetails();
        builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

        // Registers all IValidator<T> found in this assembly
        builder.Services.AddValidatorsFromAssemblyContaining<Program>();

        var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("ConnectionStrings:DefaultConnection is missing!");

        // The interceptor is resolved from DI so it can use IHttpContextAccessor (current user) and logging
        builder.Services.AddHttpContextAccessor();
        builder.Services.AddScoped<AuditLoggingInterceptor>();

        builder.Services.AddDbContext<ApplicationDbContext>((sp, options) =>
        {
            options.UseSqlServer(connectionString)
                   .AddInterceptors(sp.GetRequiredService<AuditLoggingInterceptor>());
        });

        builder.Services.AddScoped<IMovementService, MovementService>();

        // Backs the short-lived cache of the available-years list used by the chart dropdown
        builder.Services.AddMemoryCache();

        // JWT: the secret must come from user-secrets / environment variables, never from a versioned file
        var jwtSecret = builder.Configuration["JwtSettings:Secret"]
            ?? throw new InvalidOperationException("JwtSettings:Secret is missing!");
        var jwtKeyBytes = Encoding.UTF8.GetBytes(jwtSecret);
        if (jwtKeyBytes.Length < 32)
        {
            throw new InvalidOperationException("JwtSettings:Secret must be at least 32 bytes long for HS256.");
        }

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
                    IssuerSigningKey = new SymmetricSecurityKey(jwtKeyBytes)
                };
                options.Events = new JwtBearerEvents
                {
                    OnAuthenticationFailed = context =>
                    {
                        var logger = context.HttpContext.RequestServices
                            .GetRequiredService<ILoggerFactory>()
                            .CreateLogger("JwtAuthentication");
                        logger.LogWarning(context.Exception, "JWT authentication failed.");
                        return Task.CompletedTask;
                    }
                };
            });

        // Secure by default: every endpoint requires an authenticated user unless marked with AllowAnonymous
        builder.Services.AddAuthorizationBuilder()
            .SetFallbackPolicy(new Microsoft.AspNetCore.Authorization.AuthorizationPolicyBuilder()
                .RequireAuthenticatedUser()
                .Build());

        builder.Services.AddHealthChecks();

        var app = builder.Build();

        app.UseExceptionHandler();
        app.UseHttpsRedirection();

        if (isDevelopment)
        {
            app.UseSwagger();
            app.UseSwaggerUI();

            // Schema is managed through EF Core migrations; in shared environments
            // apply them with a migration bundle in the deploy pipeline instead.
            ApplyMigrations(app);
        }

        app.UseCors("Default");
        app.UseAuthentication();
        app.UseAuthorization();

        app.MapHealthChecks("/health").AllowAnonymous();
        app.MapMovementEndpoints();

        app.Run();
    }

    static void ApplyMigrations(WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();

        try
        {
            context.Database.Migrate();
            logger.LogInformation("Database migrations applied successfully.");
        }
        catch (Exception ex)
        {
            // Fail fast: running against an out-of-sync schema is worse than not starting
            logger.LogCritical(ex, "Failed to apply database migrations.");
            throw;
        }
    }
}