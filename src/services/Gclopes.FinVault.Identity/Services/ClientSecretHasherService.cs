using Konscious.Security.Cryptography;
using System.Security.Cryptography;
using System.Text;

namespace Gclopes.FinVault.Identity.Services;

public sealed class ClientSecretHasherService
{
    private const int SaltSize = 16;
    private const int HashSize = 32;

    private const int MemorySize = 65536; // 64 MB
    private const int Iterations = 3;
    private const int Parallelism = 2;

    public string HashSecret(string secret)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(secret);

        var salt = RandomNumberGenerator.GetBytes(SaltSize);
        var passwordBytes = Encoding.UTF8.GetBytes(secret);

        using var argon2 = new Argon2id(passwordBytes)
        {
            Salt = salt,
            MemorySize = MemorySize,
            Iterations = Iterations,
            DegreeOfParallelism = Parallelism
        };

        var hash = argon2.GetBytes(HashSize);

        return $"v=1$m={MemorySize},t={Iterations},p={Parallelism}${Convert.ToBase64String(salt)}${Convert.ToBase64String(hash)}";
    }

    public bool VerifySecret(string secret, string storedHash)
    {
        if (string.IsNullOrWhiteSpace(secret) ||
            string.IsNullOrWhiteSpace(storedHash))
        {
            return false;
        }

        try
        {
            var parts = storedHash.Split('$');

            if (parts.Length != 4 || parts[0] != "v=1")
                return false;

            var salt = Convert.FromBase64String(parts[2]);
            var expectedHash = Convert.FromBase64String(parts[3]);

            if (salt.Length != SaltSize || expectedHash.Length != HashSize)
                return false;

            using var argon2 = new Argon2id(Encoding.UTF8.GetBytes(secret))
            {
                Salt = salt,
                MemorySize = MemorySize,
                Iterations = Iterations,
                DegreeOfParallelism = Parallelism
            };

            var actualHash = argon2.GetBytes(HashSize);

            return CryptographicOperations.FixedTimeEquals(
                actualHash,
                expectedHash);
        }
        catch (FormatException)
        {
            return false;
        }
    }
}