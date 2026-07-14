using FluentAssertions;
using Gclopes.FinVault.Identity.Application.DTOs;
using Gclopes.FinVault.Identity.Data;
using Gclopes.FinVault.Identity.Tests.IntegrationTests.Fixtures;
using Gclopes.FinVault.Identity.Tests.IntegrationTests.Helpers;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Net.Http.Json;
using Xunit;

namespace Gclopes.FinVault.Identity.Tests.IntegrationTests;

public class AuthControllerTests(IntegrationTestFixture factory) : IClassFixture<IntegrationTestFixture>, IAsyncLifetime
{
    private readonly HttpClient _client = factory.CreateClient();

    public async Task InitializeAsync()
    {
        using var scope = factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        await DatabaseHelper.ClearTablesAsync(context);
    }

    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public async Task Login_ShouldReturnBadRequest_WhenCredentialsAreInvalid()
    {
        var invalidLogin = new { Email = "wrong@test.com", Password = "wrongpassword" };

        var response = await _client.PostAsJsonAsync("/api/v1/auth/login", invalidLogin);

        response.IsSuccessStatusCode.Should().BeFalse();
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Login_ShouldReturnOkAndToken_WhenCredentialsAreValid()
    {
        await DatabaseHelper.SeedUserAsync(factory);
        var validLogin = new { Email = "user@finvault.com", Password = "Password123!" };

        var response = await _client.PostAsJsonAsync("/api/v1/auth/login", validLogin);

        response.IsSuccessStatusCode.Should().BeTrue();
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<AuthenticationResponse>();

        result.Should().NotBeNull();
        result!.AccessToken.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task Register_ShouldReturnOk_WhenRegistrationIsValid()
    {
        var uniqueEmail = $"user_{Guid.NewGuid()}@finvault.com";
        var command = new { Email = uniqueEmail, Password = "Password123!", ConfirmPassword = "Password123!" };

        var response = await _client.PostAsJsonAsync("/api/v1/auth/register", command);

        response.IsSuccessStatusCode.Should().BeTrue();
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Register_ShouldReturnBadRequest_WhenRegistrationFails()
    {
        await DatabaseHelper.SeedUserAsync(factory);
        var command = new { Email = "user@finvault.com", Password = "Password123!", ConfirmPassword = "Password123!" };

        var response = await _client.PostAsJsonAsync("/api/v1/auth/register", command);

        response.IsSuccessStatusCode.Should().BeFalse();
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Theory]
    [InlineData("invalid-email", "Password123!@#", "Password123!@#", "Invalid Email")]
    [InlineData("newuser@finvault.com", "123", "123", "Password too short")]
    [InlineData("newuser@finvault.com", "Password123!@#", "DifferentPassword!", "Passwords do not match")]
    public async Task Register_ShouldReturnBadRequest_WhenInputIsInvalid(string email, string password, string confirmPassword, string reason)
    {
        var command = new { Email = email, Password = password, ConfirmPassword = confirmPassword };

        var response = await _client.PostAsJsonAsync("/api/v1/auth/register", command);

        response.IsSuccessStatusCode.Should().BeFalse($"because {reason}");
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}