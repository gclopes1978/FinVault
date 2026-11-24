using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace Gclopes.FinVault.CurrentAccount.DependencyInjection;

/// <summary>
/// Provides extension methods for configuring authentication and authorization.
/// </summary>
public static class AuthenticationServiceCollectionExtensions
{
    /// <summary>
    /// Registers JWT authentication and secure-by-default authorization.
    /// </summary>
    public static IServiceCollection AddCurrentAccountAuthentication(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var jwtSettings = configuration.GetSection("JwtSettings");

        var secret = jwtSettings["Secret"]
            ?? throw new InvalidOperationException(
                "JwtSettings:Secret is missing.");

        var issuer = jwtSettings["Issuer"]
            ?? throw new InvalidOperationException(
                "JwtSettings:Issuer is missing.");

        var audience = jwtSettings["Audience"]
            ?? throw new InvalidOperationException(
                "JwtSettings:Audience is missing.");

        var keyBytes = Encoding.UTF8.GetBytes(secret);

        if (keyBytes.Length < 32)
        {
            throw new InvalidOperationException(
                "JwtSettings:Secret must be at least 32 bytes long for HS256.");
        }

        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
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
                    IssuerSigningKey = new SymmetricSecurityKey(keyBytes),
                    ClockSkew = TimeSpan.Zero
                };

                options.Events = new JwtBearerEvents
                {
                    OnAuthenticationFailed = context =>
                    {
                        var logger = context.HttpContext.RequestServices
                            .GetRequiredService<ILoggerFactory>()
                            .CreateLogger("JwtAuthentication");

                        logger.LogWarning(
                            context.Exception,
                            "JWT authentication failed.");

                        return Task.CompletedTask;
                    }
                };
            });

        services.AddAuthorizationBuilder()
            .SetFallbackPolicy(
                new AuthorizationPolicyBuilder()
                    .RequireAuthenticatedUser()
                    .Build());

        return services;
    }
}