using System.Text;
using System.Text.Json;
using RabbitMQ.Client;

namespace Gclopes.FinVault.DocumentIngestion.Services;

// Bypasses MassTransit intentionally for this one queue: nothing in the solution consumes the
// MassTransit-published DocumentUploadedEvent to bridge it into the Python service, and the
// PdfParser's aio_pika consumer expects a plain, un-enveloped JSON body
// ({"documentId", "filename", "bytes"}) on a literal "pdf_processing_queue" queue — not a
// MassTransit envelope. Connection/channel are created lazily on first publish (RabbitMQ.Client
// 7.x's connection APIs are async, so they can't be opened in a constructor) and then reused.
public class PdfProcessingQueuePublisher(IConfiguration configuration) : IPdfProcessingQueuePublisher, IAsyncDisposable
{
    private const string QueueName = "pdf_processing_queue";

    private readonly IConfiguration _configuration = configuration;
    private readonly SemaphoreSlim _initLock = new(1, 1);

    private IConnection? _connection;
    private IChannel? _channel;

    private async Task<IChannel> GetChannelAsync(CancellationToken cancellationToken)
    {
        if (_channel is not null)
        {
            return _channel;
        }

        await _initLock.WaitAsync(cancellationToken);
        try
        {
            if (_channel is not null)
            {
                return _channel;
            }

            var factory = new ConnectionFactory
            {
                HostName = _configuration["RABBITMQ_HOST"] ?? "finvault-rabbitmq",
                UserName = _configuration["RABBITMQ_USER"]
                    ?? throw new InvalidOperationException("RABBITMQ_USER environment variable is missing."),
                Password = _configuration["RABBITMQ_PASS"]
                    ?? throw new InvalidOperationException("RABBITMQ_PASS environment variable is missing.")
            };

            _connection = await factory.CreateConnectionAsync(cancellationToken);
            _channel = await _connection.CreateChannelAsync(cancellationToken: cancellationToken);

            // Matches queue.declare_queue("pdf_processing_queue") on the Python side.
            await _channel.QueueDeclareAsync(
                queue: QueueName,
                durable: true,
                exclusive: false,
                autoDelete: false,
                arguments: null,
                cancellationToken: cancellationToken);

            return _channel;
        }
        finally
        {
            _initLock.Release();
        }
    }

    public async Task PublishAsync(Guid documentId, string fileName, byte[] fileBytes, CancellationToken cancellationToken = default)
    {
        var channel = await GetChannelAsync(cancellationToken);

        var message = new
        {
            sourceDocumentId = documentId.ToString(),
            filename = fileName,
            // The Python consumer base64-decodes this field before opening it with pdfplumber.
            bytes = Convert.ToBase64String(fileBytes)
        };

        var body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(message));

        var properties = new BasicProperties
        {
            ContentType = "application/json",
            DeliveryMode = DeliveryModes.Persistent
        };

        await channel.BasicPublishAsync(
            exchange: string.Empty,
            routingKey: QueueName,
            mandatory: false,
            basicProperties: properties,
            body: body,
            cancellationToken: cancellationToken);
    }

    public async ValueTask DisposeAsync()
    {
        if (_channel is not null)
        {
            await _channel.CloseAsync();
        }
        if (_connection is not null)
        {
            await _connection.CloseAsync();
        }
        GC.SuppressFinalize(this);
    }
}