using FluentValidation;
using Gclopes.FinVault.PayrollAnalysis.Data;
using Gclopes.FinVault.PayrollAnalysis.Endpoints;
using Gclopes.FinVault.PayrollAnalysis.Middlewares;
using Gclopes.FinVault.PayrollAnalysis.Services.Implementations;
using Gclopes.FinVault.PayrollAnalysis.Services.Interfaces;
using Gclopes.FinVault.PayrollAnalysis.Validators;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace Gclopes.FinVault.PayrollAnalysis;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Configure OpenAPI/Swagger documentation
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();

        // Inject the required standard baseline ProblemDetails schema generating dependencies into app runtime
        builder.Services.AddProblemDetails();

        // Register the custom global runtime crash exception handler service into the IoC injection infrastructure
        builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

        // Configure the Application Database Context with Oracle (Folha Salarial)
        builder.Services.AddDbContext<PayrollAnalysisDbContext>(options =>
            options.UseOracle(builder.Configuration.GetConnectionString("DefaultConnection")));


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
        // Register the payroll import service interface and implementation
        builder.Services.AddScoped<IPayrollImportService, PayrollImportService>();

        // Register all FluentValidation validators from this assembly
        builder.Services.AddValidatorsFromAssemblyContaining<ImportPayrollRequestValidator>();

        var app = builder.Build();

        // Intercept and route outbound error traffic profiles through the registered global exceptional framework middleware
        app.UseExceptionHandler();

        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.UseHttpsRedirection();
        app.UseCors("AllowAll");
        app.UseAuthentication();
        app.UseAuthorization();
        // Note: the /imports ingestion endpoint is called server-to-server by the PdfParser
        // pipeline inside the docker network (not routed through the Gateway to end users), and
        // this service has no JwtSettings configured yet, so no auth middleware is wired here.
        // Revisit once/if this service needs to be exposed to the frontend directly.
        app.MapPayrollEndpoints();

        app.Run();
    }
}
