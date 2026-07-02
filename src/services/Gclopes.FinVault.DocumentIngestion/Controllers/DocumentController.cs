using MassTransit;
using Microsoft.AspNetCore.Mvc;
using Gclopes.FinVault.DocumentIngestion.Models;

namespace Gclopes.FinVault.DocumentIngestion.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DocumentController(IPublishEndpoint publishEndpoint, ILogger<DocumentController> logger, IConfiguration configuration) : ControllerBase
{
    private readonly string _storagePath = configuration["StorageSettings:Path"] ?? "uploads";
    [HttpPost("upload")]
    public async Task<IActionResult> Upload(IFormFile file)
    {
        if (file == null || file.Length == 0)
        {
            return BadRequest("No file uploaded.");
        }

        // Define a storage path
        var filePath = Path.Combine(_storagePath, $"{Guid.NewGuid()}_{file.FileName}");

        // Ensure directory exists
        Directory.CreateDirectory(_storagePath);

        using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await file.CopyToAsync(stream);
        }

        // Publish event to RabbitMQ
        var eventMessage = new DocumentUploadedEvent(filePath, Guid.NewGuid(), DateTime.UtcNow);
        await publishEndpoint.Publish(eventMessage);

        logger.LogInformation("File {FileName} uploaded and event published.", file.FileName);

        return Ok(new { Message = "File uploaded successfully", Path = filePath });
    }
}