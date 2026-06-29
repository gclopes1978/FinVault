using FluentValidation;
using Gclopes.FinVault.CurrentAccount.Data;
using Gclopes.FinVault.CurrentAccount.Endpoints;
using Gclopes.FinVault.CurrentAccount.Middlewares;
using Gclopes.FinVault.CurrentAccount.Services.Implementations;
using Gclopes.FinVault.CurrentAccount.Services.Interfaces;
using Gclopes.FinVault.CurrentAccount.Validators;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace Gclopes.FinVault.CurrentAccount;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Configure OpenAPI/Swagger documentation
        builder.Services.AddControllers();
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();

        // Inject the required standard baseline ProblemDetails schema generating dependencies into app runtime
        builder.Services.AddProblemDetails();

        // Register the custom global runtime crash exception handler service into the IoC injection infrastructure
        builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

        // Configure the Application Database Context with SQL Server
        builder.Services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

        // Register the financial movement service interfaces and implementations
        builder.Services.AddScoped<IMovementService, MovementService>();

        // Register all FluentValidation validators from this assembly
        builder.Services.AddValidatorsFromAssemblyContaining<CreateMovementRequestValidator>();

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
                                    throw new InvalidOperationException("JwtSettings Secret is missing!")))
                            };
                        });

        builder.Services.AddAuthorization();

        var app = builder.Build();
        // Intercept and route outbound error traffic profiles through the registered global exceptional framework middleware
        app.UseExceptionHandler();

        // Establish safe structural gateway documentation routing pipelines when working under development states
        if (app.Environment.IsDevelopment())
        {
            // Enable the underlying application engine middleware payload processor to serve generated Swagger JSON documents
            app.UseSwagger();
            // Enable core UI dashboard presentation assets to easily explore interactive live gateway API documentation
            app.UseSwaggerUI();
        }


        app.UseHttpsRedirection();
        app.UseAuthentication();
        app.UseAuthorization();
        app.MapControllers();

        // Configure application routing endpoints
        app.MapMovementEndpoints();

        app.Run();
    }
}