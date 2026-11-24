using Gclopes.FinVault.SavingsCertificates.Endpoints;

namespace Gclopes.FinVault.SavingsCertificates.DependencyInjection;

/// <summary>
/// Provides web application pipeline extensions.
/// </summary>
public static class WebApplicationExtensions
{
    /// <summary>
    /// Configures the SavingsCertificates HTTP request pipeline.
    /// </summary>
    public static WebApplication UseSavingsCertificatesPipeline(
        this WebApplication app)
    {
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.UseHttpsRedirection();

        app.UseCors(
            CorsServiceCollectionExtensions.GetSavingsCertificatesCorsPolicyName());

        app.UseAuthentication();
        app.UseAuthorization();

        app.MapSavingsBondsEndpoints();
        app.MapSeriesRateHistoryEndpoints();

        return app;
    }
}