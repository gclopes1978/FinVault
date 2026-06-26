using System.Text.Json.Serialization;

namespace Gclopes.FinVault.Identity.Application.DTOs;

public record AuthenticationResponse
{
    [JsonPropertyName("accessToken")]
    public string AccessToken { get; init; }

    [JsonPropertyName("expiration")]
    public DateTime Expiration { get; init; }

    [JsonConstructor]
    public AuthenticationResponse(string accessToken, DateTime expiration)
    {
        AccessToken = accessToken;
        Expiration = expiration;
    }
}