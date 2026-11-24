using FluentAssertions;
using Gclopes.FinVault.Identity.Services;
using Konscious.Security.Cryptography;
using Microsoft.AspNetCore.Identity;
using System.Security.Cryptography;
using System.Text;
using Xunit;

namespace Gclopes.FinVault.Identity.Tests.UnitTests;

/// <summary>
/// Contains unit tests for the Argon2 password hasher service.
/// </summary>
public class Argon2PasswordHasherServiceTests
{
    private readonly Argon2PasswordHasherService _hasher = new();
    private readonly IdentityUser _user = new();

    /// <summary>
    /// Verifies that hashing the same password produces different hashes.
    /// </summary>
    [Fact]
    public void HashPassword_ShouldGenerateDifferentHashes_ForSamePassword()
    {
        const string password = "StrongPassword123!";

        var firstHash = _hasher.HashPassword(_user, password);
        var secondHash = _hasher.HashPassword(_user, password);

        firstHash.Should().NotBe(secondHash);
    }

    /// <summary>
    /// Verifies that the correct password produces a successful verification result.
    /// </summary>
    [Fact]
    public void VerifyHashedPassword_ShouldSucceed_ForCorrectPassword()
    {
        const string password = "StrongPassword123!";

        var hash = _hasher.HashPassword(_user, password);

        var result = _hasher.VerifyHashedPassword(_user, hash, password);

        result.Should().Be(PasswordVerificationResult.Success);
    }

    /// <summary>
    /// Verifies that an incorrect password is rejected.
    /// </summary>
    [Fact]
    public void VerifyHashedPassword_ShouldFail_ForIncorrectPassword()
    {
        var hash = _hasher.HashPassword(_user, "StrongPassword123!");

        var result = _hasher.VerifyHashedPassword(
            _user,
            hash,
            "WrongPassword123!");

        result.Should().Be(PasswordVerificationResult.Failed);
    }

    /// <summary>
    /// Verifies that malformed hashes are rejected.
    /// </summary>
    [Fact]
    public void VerifyHashedPassword_ShouldFail_ForMalformedHash()
    {
        var result = _hasher.VerifyHashedPassword(
            _user,
            "invalid-hash",
            "StrongPassword123!");

        result.Should().Be(PasswordVerificationResult.Failed);
    }

    /// <summary>
    /// Verifies that hashes using older valid parameters request rehashing.
    /// </summary>
    [Fact]
    public void VerifyHashedPassword_ShouldRequestRehash_ForOlderParameters()
    {
        const string password = "StrongPassword123!";

        var hash = GenerateHash(password, 32768, 3, 2);

        var result = _hasher.VerifyHashedPassword(_user, hash, password);

        result.Should().Be(PasswordVerificationResult.SuccessRehashNeeded);
    }

    /// <summary>
    /// Verifies that hashes containing excessive parameters are rejected.
    /// </summary>
    [Fact]
    public void VerifyHashedPassword_ShouldFail_ForExcessiveParameters()
    {
        var result = _hasher.VerifyHashedPassword(
            _user,
            "v=1$m=999999,t=3,p=2$YWJjZGVmZ2hpamtsbW5vcA==$YWJjZGVmZ2hpamtsbW5vcA==",
            "StrongPassword123!");

        result.Should().Be(PasswordVerificationResult.Failed);
    }

    private static string GenerateHash(
        string password,
        int memorySize,
        int iterations,
        int parallelism)
    {
        var salt = RandomNumberGenerator.GetBytes(16);

        using var argon2 = new Argon2id(Encoding.UTF8.GetBytes(password))
        {
            Salt = salt,
            MemorySize = memorySize,
            Iterations = iterations,
            DegreeOfParallelism = parallelism
        };

        var hash = argon2.GetBytes(32);

        return $"v=1$m={memorySize},t={iterations},p={parallelism}${Convert.ToBase64String(salt)}${Convert.ToBase64String(hash)}";
    }
}