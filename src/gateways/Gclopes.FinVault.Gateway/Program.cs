using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Polly;
using Polly.CircuitBreaker;
using Polly.Retry;
using System.Text;

namespace Gclopes.FinVault.Gateway;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // CORS
        builder.Services.AddCors(options =>
        {
            options.AddPolicy("AngularFrontendPolicy", policy =>
            {
                policy.WithOrigins("http://localhost:4200")
                      .AllowAnyMethod()
                      .AllowAnyHeader();
            });
        });

        // Swagger
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new OpenApiInfo { Title = "FinVault Gateway API", Version = "v1" });

            options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Name = "Authorization",
                Type = SecuritySchemeType.Http,
                Scheme = "Bearer",
                BearerFormat = "JWT",
                In = ParameterLocation.Header,
                Description = "Insira o token JWT: Bearer {token}"
            });

            options.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme { Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" } },
                    Array.Empty<string>()
                }
            });
        });

        // JWT Authentication
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
            });

        // Authorization + Policy for YARP
        builder.Services.AddAuthorization(options =>
        {
            options.AddPolicy("Default", policy =>
            {
                policy.RequireAuthenticatedUser();
            });
        });

        // YARP
        builder.Services.AddReverseProxy()
                        .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

        // Resilience
        builder.Services.AddResiliencePipeline("default", pipeline =>
        {
            pipeline.AddRetry(new RetryStrategyOptions { MaxRetryAttempts = 2 });
            pipeline.AddCircuitBreaker(new CircuitBreakerStrategyOptions());
        });

        var app = builder.Build();

        // Swagger UI
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/api/v1/accounts/swagger/v1/swagger.json", "Current Account API");
                c.SwaggerEndpoint("/api/v1/document/swagger/v1/swagger.json", "Document Ingestion API");
                c.SwaggerEndpoint("/api/v1/investments/swagger/v1/swagger.json", "Investments API");
                c.SwaggerEndpoint("/api/v1/pension-plans/swagger/v1/swagger.json", "Pension Plans API");
                c.RoutePrefix = "swagger";
            });
        }

        // ORDER MATTERS — this is the correct pipeline
        app.UseRouting();

        app.UseCors("AngularFrontendPolicy");

        app.UseAuthentication();
        app.UseAuthorization();

        // YARP
        app.MapReverseProxy();

        app.Run();
    }
}
