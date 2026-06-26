using FluentAssertions;
using Gclopes.FinVault.Identity.Application.DTOs;
using Gclopes.FinVault.Identity.Data;
using Gclopes.FinVault.Identity.Tests.IntegrationTests.Fixtures;
using Gclopes.FinVault.Identity.Tests.IntegrationTests.Helpers;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Net.Http.Json;

namespace Gclopes.FinVault.Identity.Tests.IntegrationTests;

public class AuthControllerTests(IntegrationTestFixture factory) : IClassFixture<IntegrationTestFixture>, IAsyncLifetime
{
    private readonly HttpClient _client = factory.CreateClient();

    ValueTask IAsyncLifetime.InitializeAsync()
    {
        // Criamos o escopo, mas não o descartamos antes da operação terminar.
        // O IAsyncLifetime espera um Task, por isso vamos fazer o trabalho de forma assíncrona.
        var scope = factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        // Passamos o contexto para o helper.
        // NOTA: Como o 'scope' está fora do 'using', ele só será descartado quando o método terminar.
        // Como queremos que o contexto sobreviva à chamada, podes descartar o scope aqui? 
        // Não. O ideal é descartar o scope APÓS a operação de limpeza.

        return new ValueTask(PerformCleanup(scope, context));
    }

    private static async Task PerformCleanup(IServiceScope scope, ApplicationDbContext context)
    {
        try
        {
            await DatabaseHelper.ClearTablesAsync(context);
        }
        finally
        {
            // Só descartamos o scope (e consequentemente o contexto) 
            // depois da base de dados estar limpa.
            scope.Dispose();
        }
    }

    // Explicit implementation for IAsyncDisposable
    ValueTask IAsyncDisposable.DisposeAsync()
    {
        GC.SuppressFinalize(this);
        // Se não precisares de cleanup manual de recursos, apenas completas a tarefa
        return ValueTask.CompletedTask;
    }
    
    [Fact]
    public async Task Login_ShouldReturnBadRequest_WhenCredentialsAreInvalid()
    {
        // Arrange
        var invalidLogin = new { Email = "wrong@test.com", Password = "wrongpassword" };

        // Act
        var response = await _client.PostAsJsonAsync("/api/v1/auth/login", invalidLogin, cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        response.IsSuccessStatusCode.Should().BeFalse();
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Login_ShouldReturnOkAndToken_WhenCredentialsAreValid()
    {
        // Arrange
        // Nota: Precisas de ter um utilizador criado na BD antes deste teste
        // Se estiveres a usar InMemory, podes criar o utilizador no construtor ou num método de Setup

        await DatabaseHelper.SeedUserAsync(factory);

        var validLogin = new { Email = "user@finvault.com", Password = "Password123!" };

        // Act
        var response = await _client.PostAsJsonAsync("/api/v1/auth/login", validLogin, TestContext.Current.CancellationToken);

        // Assert
        response.IsSuccessStatusCode.Should().BeTrue();
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        // Deserialize directly into the response DTO
        var result = await response.Content.ReadFromJsonAsync<AuthenticationResponse>(cancellationToken: TestContext.Current.CancellationToken);

        // Assertions
        result.Should().NotBeNull();
        result!.AccessToken.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task Register_ShouldReturnOk_WhenRegistrationIsValid()
    {
        // Arrange: Define valid registration command
        var uniqueEmail = $"user_{Guid.NewGuid()}@finvault.com";
        var command = new
        {
            Email = uniqueEmail,
            Password = "Password123!",
            ConfirmPassword = "Password123!"
        };
       
        // Act
        var response = await _client.PostAsJsonAsync("/api/v1/auth/register", command, TestContext.Current.CancellationToken);

        // Force the reading of the response body before the assertion
        var errorContent = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);

        // Assertions
        response.IsSuccessStatusCode.Should().BeTrue("because the registration should succeed, but failed with: {0}", errorContent);
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        // Read and verify the success message
        var content = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
        content.Should().Contain("User registered successfully.");
    }

    [Fact]
    public async Task Register_ShouldReturnBadRequest_WhenRegistrationFails()
    {
        // Arrange: Create a user first so registration fails on duplicate email
        await DatabaseHelper.SeedUserAsync(factory);
        var command = new
        {
            Email = "user", // This user already exists
            Password = "Password123!",
            ConfirmPassword = "Password123!"
        };

        // Act: Perform the registration request
        var response = await _client.PostAsJsonAsync("/api/v1/auth/register", command, TestContext.Current.CancellationToken);

        // Assert: Verify bad request response
        response.IsSuccessStatusCode.Should().BeFalse();
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Theory]
    [InlineData("invalid-email", "Password123!@#", "Password123!@#", "Invalid Email")]
    [InlineData("newuser@finvault.com", "123", "123", "Password too short")]
    [InlineData("newuser@finvault.com", "Password123!@#", "DifferentPassword!", "Passwords do not match")]
    public async Task Register_ShouldReturnBadRequest_WhenInputIsInvalid(
    string email,
    string password,
    string confirmPassword,
    string reason)
    {
        // Arrange: Define the invalid command based on theory parameters
        var command = new
        {
            Email = email,
            Password = password,
            ConfirmPassword = confirmPassword
        };

        // Act: Perform the registration request
        var response = await _client.PostAsJsonAsync("/api/v1/auth/register", command, TestContext.Current.CancellationToken);

        // Assert: Verify that the request failed as expected
        response.IsSuccessStatusCode.Should().BeFalse($"because {reason}");
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}