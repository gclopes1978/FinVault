using Gclopes.FinVault.Identity.Data;
using Gclopes.FinVault.Identity.Tests.IntegrationTests.Fixtures;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Gclopes.FinVault.Identity.Tests.IntegrationTests.Helpers
{
    public class DatabaseHelper
    {
        public static async Task ClearTablesAsync(ApplicationDbContext context)
        {
            // Usar uma transação força o EF a manter a ligação aberta 
            // e isola a operação de limpeza.
            var strategy = context.Database.CreateExecutionStrategy();

            await strategy.ExecuteAsync(async () =>
            {
                using var transaction = await context.Database.BeginTransactionAsync();
                try
                {
                    await context.Database.ExecuteSqlRawAsync("DELETE FROM AspNetUsers");
                    await context.Database.ExecuteSqlRawAsync("DELETE FROM AspNetRoles");

                    await transaction.CommitAsync();
                }
                catch
                {
                    await transaction.RollbackAsync();
                    throw;
                }
            });
        }

        public static async Task SeedUserAsync(IntegrationTestFixture factory)
        {
            using var scope = factory.Services.CreateScope();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();
            var user = new IdentityUser { UserName = "user@finvault.com", Email = "user@finvault.com" };
            await userManager.CreateAsync(user, "Password123!");
        }
    }
}
