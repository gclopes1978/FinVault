using Microsoft.OpenApi.Models;
using Polly.CircuitBreaker;
using Polly.Retry;
using Polly;

namespace Gclopes.FinVault.Gateway;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Services.AddCors(options =>
        {
            options.AddPolicy("AngularFrontendPolicy", policy =>
            {
                policy.WithOrigins("http://localhost:4200")
                      .AllowAnyMethod()
                      .AllowAnyHeader();
            });
        });

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

        builder.Services.AddReverseProxy()
                        .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

        builder.Services.AddResiliencePipeline("default", pipeline =>
        {
            pipeline.AddRetry(new RetryStrategyOptions { MaxRetryAttempts = 2 });
            pipeline.AddCircuitBreaker(new CircuitBreakerStrategyOptions());
        });

        var app = builder.Build();

        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                // Agregação manual (mais fiável e sem dependências extras)
                // Aponta para a rota pública que o YARP expõe
                c.SwaggerEndpoint("/api/v1/accounts/swagger/v1/swagger.json", "Current Account API");
                c.SwaggerEndpoint("/api/v1/Document/swagger/v1/swagger.json", "Document Ingestion API");
                c.SwaggerEndpoint("/api/v1/investments/swagger/v1/swagger.json", "Investments API");
                c.SwaggerEndpoint("/api/v1/pension-plans/swagger/v1/swagger.json", "Pension Plans API");

                c.RoutePrefix = "swagger";
            });
        }

        app.UseRouting();
        app.UseCors("AngularFrontendPolicy");
        app.MapReverseProxy();

        app.Run();
    }
}