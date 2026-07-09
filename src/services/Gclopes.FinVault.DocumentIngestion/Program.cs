using MassTransit;
using Microsoft.OpenApi.Models;
using Gclopes.FinVault.DocumentIngestion.Services;

var builder = WebApplication.CreateBuilder(args);

// 1. Add Services
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Gclopes.FinVault.DocumentIngestion API", Version = "v1" });
});

// 2. RabbitMQ Configuration
var rabbitHost = builder.Configuration["RABBITMQ_HOST"] ?? "finvault-rabbitmq";
var rabbitUser = builder.Configuration["RABBITMQ_USER"]
    ?? throw new InvalidOperationException("RABBITMQ_USER environment variable is missing.");
var rabbitPass = builder.Configuration["RABBITMQ_PASS"]
    ?? throw new InvalidOperationException("RABBITMQ_PASS environment variable is missing.");

builder.Services.AddMassTransit(x =>
{
    // Formats endpoint names using kebab-case (e.g., submit-order instead of SubmitOrder)
    x.SetKebabCaseEndpointNameFormatter();

    x.UsingRabbitMq((context, cfg) =>
    {
        cfg.Host(rabbitHost, "/", h =>
        {
            h.Username(rabbitUser);
            h.Password(rabbitPass);
        });

        // Automatically configures endpoints for any discovered consumers
        cfg.ConfigureEndpoints(context);
    });
});

// Bridges uploaded documents into the raw queue the Python PdfParser consumes from.
// Singleton: keeps a single AMQP connection/channel open for the app's lifetime instead of
// reconnecting on every upload.
builder.Services.AddSingleton<IPdfProcessingQueuePublisher, PdfProcessingQueuePublisher>();

var app = builder.Build();

// 3. Pipeline Configuration
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();