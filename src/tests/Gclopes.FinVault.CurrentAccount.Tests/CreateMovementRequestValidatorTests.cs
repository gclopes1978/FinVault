using FluentAssertions;
using Gclopes.FinVault.CurrentAccount.Models;
using Gclopes.FinVault.CurrentAccount.Validators;

namespace Gclopes.FinVault.CurrentAccount.Tests;

public class CreateMovementRequestValidatorTests
{
    private readonly CreateMovementRequestValidator _validator;

    public CreateMovementRequestValidatorTests()
    {
        // Setup the unit test environment
        _validator = new CreateMovementRequestValidator();
    }

    [Fact]
    public async Task ValidateAsync_WithValidRequest_ShouldNotHaveValidationErrors()
    {
        // Arrange
        var request = new CreateMovementRequest(
            Amount: 150.00m,
            Type: "Credit",
            Description: "Salary payment",
            Category: "Income",
            TransactionDate: DateTime.UtcNow,
            ValueDate: DateTime.UtcNow
        );

        // Act
        var result = await _validator.ValidateAsync(request);

        // Assert using FluentAssertions
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-50.50)]
    public async Task ValidateAsync_WithInvalidOrNegativeAmount_ShouldFailValidation(decimal invalidAmount)
    {
        // Arrange
        var request = new CreateMovementRequest(
            Amount: invalidAmount,
            Type: "Credit",
            Description: "Valid Description",
            Category: "Valid Category",
            TransactionDate: DateTime.UtcNow,
            ValueDate: DateTime.UtcNow
        );

        // Act
        var result = await _validator.ValidateAsync(request);

        // Assert using FluentAssertions
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(x => x.PropertyName == nameof(CreateMovementRequest.Amount));
    }

    [Fact]
    public async Task ValidateAsync_WithInvalidType_ShouldFailValidation()
    {
        // Arrange
        var request = new CreateMovementRequest(
            Amount: 10.00m,
            Type: "InvalidType", // Must be Credit or Debit
            Description: "Valid Description",
            Category: "Valid Category",
            TransactionDate: DateTime.UtcNow,
            ValueDate: DateTime.UtcNow
        );

        // Act
        var result = await _validator.ValidateAsync(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(x => x.PropertyName == nameof(CreateMovementRequest.Type));
    }
}