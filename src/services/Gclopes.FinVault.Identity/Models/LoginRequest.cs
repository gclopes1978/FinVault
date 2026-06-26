namespace Gclopes.FinVault.Identity.Models
{
    /// <summary>
    /// Represents the custom login request data transfer object.
    /// </summary>
    /// <param name="Email">The user email address.</param>
    /// <param name="Password">The user password.</param>
    public record LoginRequest(string Email, string Password);
}
