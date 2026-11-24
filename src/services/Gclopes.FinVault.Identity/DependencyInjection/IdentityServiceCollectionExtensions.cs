using Gclopes.FinVault.Identity.Services;
using Microsoft.AspNetCore.Identity;

namespace Gclopes.FinVault.Identity.DependencyInjection;

/// <summary>
/// Provides extension methods for registering ASP.NET Core Identity services.
/// </summary>
public static class IdentityServiceCollectionExtensions
{
    /// <summary>
    /// Registers and configures ASP.NET Core Identity services.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddFinVaultIdentity(
        this IServiceCollection services)
    {
        services.AddIdentity<IdentityUser, IdentityRole>(options =>
        {
            // Password policy
            options.Password.RequiredLength = 12;
            options.Password.RequireDigit = true;
            options.Password.RequireLowercase = true;
            options.Password.RequireUppercase = true;
            options.Password.RequireNonAlphanumeric = true;
            options.Password.RequiredUniqueChars = 4;

            // Account lockout policy
            options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
            options.Lockout.MaxFailedAccessAttempts = 5;
            options.Lockout.AllowedForNewUsers = true;

            // User settings
            options.User.RequireUniqueEmail = true;
        })
        .AddEntityFrameworkStores<Data.ApplicationDbContext>()
        .AddDefaultTokenProviders();

        services.AddScoped<IPasswordHasher<IdentityUser>, Argon2PasswordHasherService>();

        return services;
    }
}