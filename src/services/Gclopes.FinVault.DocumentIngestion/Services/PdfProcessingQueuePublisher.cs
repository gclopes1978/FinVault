using System.Text;
using System.Text.Json;
using RabbitMQ.Client;

namespace Gclopes.FinVault.DocumentIngestion.Services;

// Bypasses MassTransit intentionally for this one queue: nothing in the solution consumes the
// MassTransit-published DocumentUploadedEvent to bridge it into the Python service, and the
// PdfParser's aio_pika consumer expects a plain, un-enveloped JSON body
// ({"documentId", "filename", "bytes"}) on a literal "pdf_processing_queue" queue — not a
// MassTransit envelope. A single long-lived connection/channel is kept open for the lifetime of
// the app (registered as a singleton) rather than reconnecting per upload.
public class PdfProcessingQueuePublisher : IPdfProcessingQueuePublisher, IDisposable
{
    private const string QueueName = "pdf_processing_queue";

    private readonly IConnection _connection;
    private readonly IModel _channel;

    public PdfProcessingQueuePublisher(IConfiguration configuration)
    {
        var factory = new ConnectionFactory
        {
            HostName = configuration["RABBITMQ_HOST"] ?? "finvault-rabbitmq",
            UserName = configuration["RABBITMQ_USER"]
                ?? throw new InvalidOperationException("RABBITMQ_USER environment variable is missing."),
            Password = configuration["RABBITMQ_PASS"]
                ?? throw new InvalidOperationException("RABBITMQ_PASS environment variable is missing.")
        };

        _connection = factory.CreateConnection();
        _channel = _connection.CreateModel();

        // Matches the queue.declare_queue("pdf_processing_queue") call on the Python side
        // (default aio_pika arguments: durable=false, exclusive=false, auto_delete=false).
        _channel.QueueDeclare(queue: QueueName, durable: false, exclusive: false, autoDelete: false);
    }

    public Task PublishAsync(Guid documentId, string fileName, byte[] fileBytes, CancellationToken cancellationToken = default)
    {
        var message = new
        {
            documentId = documentId.ToString(),
            filename = fileName,
            // The Python consumer base64-decodes this field before opening it with pdfplumber.
            bytes = Convert.ToBase64String(fileBytes)
        };

        var body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(message));

        var properties = _channel.CreateBasicProperties();
        properties.ContentType = "application/json";
        properties.DeliveryMode = 2; // persistent

        _channel.BasicPublish(exchange: string.Empty, routingKey: QueueName, basicProperties: properties, body: body);

        return Task.CompletedTask;
    }

    public void Dispose()
    {
        _channel?.Close();
        _connection?.Close();
        GC.SuppressFinalize(this);
    }
}
