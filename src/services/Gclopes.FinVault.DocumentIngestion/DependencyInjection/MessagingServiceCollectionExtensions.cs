using MassTransit;

namespace Gclopes.FinVault.DocumentIngestion.DependencyInjection;

/// <summary>
/// Provides extension methods for configuring messaging services.
/// </summary>
public static class MessagingServiceCollectionExtensions
{
    /// <summary>
    /// Registers MassTransit with RabbitMQ.
    /// </summary>
    public static IServiceCollection AddDocumentIngestionMessaging(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var rabbitHost = configuration["RABBITMQ_HOST"]
            ?? "finvault-rabbitmq";

        var rabbitUser = configuration["RABBITMQ_USER"]
            ?? throw new InvalidOperationException(
                "RABBITMQ_USER environment variable is missing.");

        var rabbitPassword = configuration["RABBITMQ_PASS"]
            ?? throw new InvalidOperationException(
                "RABBITMQ_PASS environment variable is missing.");

        services.AddMassTransit(x =>
        {
            x.SetKebabCaseEndpointNameFormatter();

            x.UsingRabbitMq((context, cfg) =>
            {
                cfg.Host(rabbitHost, "/", host =>
                {
                    host.Username(rabbitUser);
                    host.Password(rabbitPassword);
                });

                cfg.ConfigureEndpoints(context);
            });
        });

        return services;
    }
}