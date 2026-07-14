using FluentAssertions;
using Gclopes.FinVault.Identity.Application.Commands;
using Gclopes.FinVault.Identity.Application.Handlers;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace Gclopes.FinVault.Identity.Tests.UnitTests;

public class RegisterUserHandlerTests
{
    private readonly Mock<UserManager<IdentityUser>> _userManagerMock;
    private readonly RegisterUserHandler _handler;

    public RegisterUserHandlerTests()
    {
        _userManagerMock = CreateUserManagerMock();
        _handler = new RegisterUserHandler(_userManagerMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnTrue_WhenUserIsCreated()
    {
        // Arrange
        _userManagerMock.Setup(x => x.CreateAsync(It.IsAny<IdentityUser>(), It.IsAny<string>()))
                        .ReturnsAsync(IdentityResult.Success);

        var command = new RegisterUserCommand("test@finvault.com", "P@ssword123!", "P@ssword123!");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_ShouldReturnFalse_WhenUserCreationFails()
    {
        // Arrange
        var error = new IdentityError { Code = "PasswordTooShort", Description = "Password is too short." };
        _userManagerMock.Setup(x => x.CreateAsync(It.IsAny<IdentityUser>(), It.IsAny<string>()))
                        .ReturnsAsync(IdentityResult.Failed(error));

        var command = new RegisterUserCommand("test@finvault.com", "123", "123");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().BeFalse();
    }

    private static Mock<UserManager<IdentityUser>> CreateUserManagerMock()
    {
        return new Mock<UserManager<IdentityUser>>(
            Mock.Of<IUserStore<IdentityUser>>(),
            Mock.Of<IOptions<IdentityOptions>>(),
            Mock.Of<IPasswordHasher<IdentityUser>>(),
            Array.Empty<IUserValidator<IdentityUser>>(),
            Array.Empty<IPasswordValidator<IdentityUser>>(),
            Mock.Of<ILookupNormalizer>(),
            Mock.Of<IdentityErrorDescriber>(),
            Mock.Of<IServiceProvider>(),
            Mock.Of<ILogger<UserManager<IdentityUser>>>()
        );
    }
}