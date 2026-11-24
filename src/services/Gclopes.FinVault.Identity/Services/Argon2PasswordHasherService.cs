using Konscious.Security.Cryptography;
using Microsoft.AspNetCore.Identity;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;

namespace Gclopes.FinVault.Identity.Services;

/// <summary>
/// Provides password hashing and verification using the Argon2id algorithm.
/// </summary>
public sealed class Argon2PasswordHasherService : IPasswordHasher<IdentityUser>
{
    private const int SaltSize = 16;
    private const int HashSize = 32;

    private const int MemorySize = 65536;
    private const int Iterations = 3;
    private const int Parallelism = 2;

    private const int MaxMemorySize = 262144;
    private const int MaxIterations = 10;
    private const int MaxParallelism = 8;

    /// <summary>
    /// Generates an Argon2id hash for the specified password.
    /// </summary>
    /// <param name="user">The user whose password is being hashed.</param>
    /// <param name="password">The password to hash.</param>
    /// <returns>The encoded Argon2id hash.</returns>
    public string HashPassword(IdentityUser user, string password)
    {
        ArgumentNullException.ThrowIfNull(user);
        ArgumentException.ThrowIfNullOrWhiteSpace(password);

        var salt = RandomNumberGenerator.GetBytes(SaltSize);
        var passwordBytes = Encoding.UTF8.GetBytes(password);

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

    /// <summary>
    /// Verifies a password against a previously generated Argon2id hash.
    /// </summary>
    /// <param name="user">The user whose password is being verified.</param>
    /// <param name="hashedPassword">The previously stored password hash.</param>
    /// <param name="providedPassword">The password provided for verification.</param>
    /// <returns>The password verification result.</returns>
    public PasswordVerificationResult VerifyHashedPassword(
        IdentityUser user,
        string hashedPassword,
        string providedPassword)
    {
        ArgumentNullException.ThrowIfNull(user);

        if (string.IsNullOrWhiteSpace(hashedPassword) ||
            string.IsNullOrWhiteSpace(providedPassword))
        {
            return PasswordVerificationResult.Failed;
        }

        try
        {
            var parts = hashedPassword.Split('$');

            if (parts.Length != 4 || parts[0] != "v=1")
            {
                return PasswordVerificationResult.Failed;
            }

            var parameters = ParseParameters(parts[1]);

            if (parameters == null)
            {
                return PasswordVerificationResult.Failed;
            }

            var salt = Convert.FromBase64String(parts[2]);
            var expectedHash = Convert.FromBase64String(parts[3]);

            if (salt.Length != SaltSize || expectedHash.Length != HashSize)
            {
                return PasswordVerificationResult.Failed;
            }

            using var argon2 = new Argon2id(
                Encoding.UTF8.GetBytes(providedPassword))
            {
                Salt = salt,
                MemorySize = parameters.Value.Memory,
                Iterations = parameters.Value.Iterations,
                DegreeOfParallelism = parameters.Value.Parallelism
            };

            var actualHash = argon2.GetBytes(HashSize);

            if (!CryptographicOperations.FixedTimeEquals(
                actualHash,
                expectedHash))
            {
                return PasswordVerificationResult.Failed;
            }

            var requiresRehash =
                parameters.Value.Memory != MemorySize ||
                parameters.Value.Iterations != Iterations ||
                parameters.Value.Parallelism != Parallelism;

            return requiresRehash
                ? PasswordVerificationResult.SuccessRehashNeeded
                : PasswordVerificationResult.Success;
        }
        catch (FormatException)
        {
            return PasswordVerificationResult.Failed;
        }
    }

    /// <summary>
    /// Parses and validates the Argon2id parameters stored in a password hash.
    /// </summary>
    /// <param name="value">The encoded parameter string.</param>
    /// <returns>The validated parameters, or null if invalid.</returns>
    private static (int Memory, int Iterations, int Parallelism)? ParseParameters(
        string value)
    {
        var parameters = value.Split(',');

        if (parameters.Length != 3)
        {
            return null;
        }

        var memory = ParseParameter(parameters[0], "m=");
        var iterations = ParseParameter(parameters[1], "t=");
        var parallelism = ParseParameter(parameters[2], "p=");

        if (memory == null || iterations == null || parallelism == null)
        {
            return null;
        }

        if (memory < 8 * parallelism ||
            memory > MaxMemorySize ||
            iterations < 1 ||
            iterations > MaxIterations ||
            parallelism < 1 ||
            parallelism > MaxParallelism)
        {
            return null;
        }

        return (memory.Value, iterations.Value, parallelism.Value);
    }

    /// <summary>
    /// Parses an individual Argon2id parameter.
    /// </summary>
    /// <param name="value">The encoded parameter.</param>
    /// <param name="prefix">The expected parameter prefix.</param>
    /// <returns>The parsed integer, or null if invalid.</returns>
    private static int? ParseParameter(string value, string prefix)
    {
        if (!value.StartsWith(prefix, StringComparison.Ordinal))
        {
            return null;
        }

        return int.TryParse(
            value.AsSpan(prefix.Length),
            NumberStyles.None,
            CultureInfo.InvariantCulture,
            out var result)
            ? result
            : null;
    }
}