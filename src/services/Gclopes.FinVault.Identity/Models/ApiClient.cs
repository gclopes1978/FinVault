namespace Gclopes.FinVault.Identity.Models;

/// <summary>
/// Represents an application authorized to authenticate using client credentials.
/// </summary>
public class ApiClient
{
    /// <summary>
    /// Gets or sets the internal unique identifier.
    /// </summary>
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// Gets or sets the public client identifier.
    /// </summary>
    public required string ClientId { get; set; }

    /// <summary>
    /// Gets or sets the Argon2id hash of the client secret.
    /// </summary>
    public required string ClientSecretHash { get; set; }

    /// <summary>
    /// Gets or sets the application name.
    /// </summary>
    public required string Name { get; set; }

    /// <summary>
    /// Gets or sets whether the client is allowed to authenticate.
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Gets or sets the UTC creation timestamp.
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}