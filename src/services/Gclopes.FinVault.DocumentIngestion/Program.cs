using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using Gclopes.FinVault.DocumentIngestion.Data;
using Gclopes.FinVault.DocumentIngestion.Hubs;
using Gclopes.FinVault.DocumentIngestion.Services;

var builder = WebApplication.CreateBuilder(args);

// 1. Add Services
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Gclopes.FinVault.DocumentIngestion API", Version = "v1" });
});

// Pushes "documentProcessed" events to the PDF Upload Centre once the PdfParser reports a
// document as finished, so the import history updates live instead of polling.
builder.Services.AddSignalR();

// Configure the Application Database Context with SQL Server, used to persist the
// upload/processing history (UploadedDocument) shown in the PDF Upload Centre.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    options.UseSqlServer(connectionString);
    options.ConfigureWarnings(w => w.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.RelationalEventId.PendingModelChangesWarning));
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

ApplyMigrations(app);

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.MapHub<DocumentProcessingHub>("/api/document/hub");

app.Run();

static void ApplyMigrations(WebApplication app)
{
    using var scope = app.Services.CreateScope();
    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();

    try
    {
        // Same approach used by the other services in this solution: EnsureCreated() maps the
        // current DbContext model straight onto the database without relying on a migrations
        // history table. No-ops if the table already exists.
        if (context.Database.EnsureCreated())
        {
            logger.LogInformation("Database created successfully.");
        }
        else
        {
            logger.LogInformation("Database already exists. Schema verification complete.");
        }
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "An error occurred while synchronizing the database schema.");
    }
}