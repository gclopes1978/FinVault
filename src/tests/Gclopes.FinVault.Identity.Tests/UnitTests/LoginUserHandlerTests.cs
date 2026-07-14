using FluentAssertions;
using Gclopes.FinVault.Identity.Application.Commands;
using Gclopes.FinVault.Identity.Application.Handlers;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Moq;
using Xunit;

namespace Gclopes.FinVault.Identity.Tests.UnitTests;

public class LoginUserHandlerTests
{
    private readonly Mock<SignInManager<IdentityUser>> _signInManagerMock;
    private readonly Mock<UserManager<IdentityUser>> _userManagerMock;
    private readonly Mock<IConfiguration> _configMock;
    private readonly LoginUserHandler _handler;

    public LoginUserHandlerTests()
    {
        // Setup Mocks
        var userStoreMock = new Mock<IUserStore<IdentityUser>>();
        _userManagerMock = new Mock<UserManager<IdentityUser>>(userStoreMock.Object, null!, null!, null!, null!, null!, null!, null!, null!);

        var contextAccessor = new Mock<Microsoft.AspNetCore.Http.IHttpContextAccessor>();
        var userPrincipalFactory = new Mock<IUserClaimsPrincipalFactory<IdentityUser>>();
        _signInManagerMock = new Mock<SignInManager<IdentityUser>>(_userManagerMock.Object, contextAccessor.Object, userPrincipalFactory.Object, null!, null!, null!, null!);

        _configMock = new Mock<IConfiguration>();

        // Mocking Configuration
        _configMock.Setup(c => c["Jwt:Key"]).Returns("SuperSecretKeyThatMustBeAtLeast32CharactersLong");
        _configMock.Setup(c => c["Jwt:Issuer"]).Returns("TestIssuer");
        _configMock.Setup(c => c["Jwt:Audience"]).Returns("TestAudience");

        _handler = new LoginUserHandler(_signInManagerMock.Object, _userManagerMock.Object, _configMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnNull_WhenCredentialsAreInvalid()
    {
        // Arrange
        var command = new LoginUserCommand("test@test.com", "wrongpassword");
        _signInManagerMock.Setup(x => x.PasswordSignInAsync(command.Email, command.Password, false, false))
                          .ReturnsAsync(SignInResult.Failed);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task Handle_ShouldReturnToken_WhenCredentialsAreValid()
    {
        // Arrange
        var email = "test@test.com";
        var command = new LoginUserCommand(email, "password123");
        var user = new IdentityUser { UserName = email, Email = email };

        // Mock: Login bem sucedido
        _signInManagerMock.Setup(x => x.PasswordSignInAsync(command.Email, command.Password, false, false))
                          .ReturnsAsync(SignInResult.Success);

        // Mock: Encontrar utilizador após sucesso
        _userManagerMock.Setup(x => x.FindByEmailAsync(email))
                        .ReturnsAsync(user);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.AccessToken.Should().NotBeNullOrEmpty();
        result.Expiration.Should().BeAfter(DateTime.UtcNow);
    }
}