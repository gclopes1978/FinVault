using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace Gclopes.FinVault.SavingsCertificates.DependencyInjection;

/// <summary>
/// Provides authentication and authorization service registration extensions.
/// </summary>
public static class AuthenticationServiceCollectionExtensions
{
    /// <summary>
    /// Registers JWT authentication and authorization.
    /// </summary>
    public static IServiceCollection AddSavingsCertificatesAuthentication(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var jwtSecret = configuration["JwtSettings:Secret"]
            ?? throw new InvalidOperationException(
                "JwtSettings:Secret is missing.");

        var issuer = configuration["JwtSettings:Issuer"]
            ?? throw new InvalidOperationException(
                "JwtSettings:Issuer is missing.");

        var audience = configuration["JwtSettings:Audience"]
            ?? throw new InvalidOperationException(
                "JwtSettings:Audience is missing.");

        var jwtKeyBytes = Encoding.UTF8.GetBytes(jwtSecret);

        if (jwtKeyBytes.Length < 32)
        {
            throw new InvalidOperationException(
                "JwtSettings:Secret must be at least 32 bytes long for HS256.");
        }

        services
            .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ClockSkew = TimeSpan.Zero,
                    ValidIssuer = issuer,
                    ValidAudience = audience,
                    IssuerSigningKey = new SymmetricSecurityKey(jwtKeyBytes)
                };
            });

        services.AddAuthorization();

        return services;
    }
}