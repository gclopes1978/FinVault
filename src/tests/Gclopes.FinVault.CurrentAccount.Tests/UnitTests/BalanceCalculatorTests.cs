using FluentAssertions;
using Gclopes.FinVault.CurrentAccount.Domain;

namespace Gclopes.FinVault.CurrentAccount.Tests.UnitTests;

public class BalanceCalculatorTests
{
    [Fact]
    public void CalculateFinalBalance_ShouldReturnCorrectBalance_WhenTransactionsAreValid()
    {
        // 1. Arrange (Preparar)
        var initialBalance = 1000.00m;
        var movements = new List<(decimal Amount, string Type)>
        {
            (200.00m, "C"), // +200 -> 1200
            (50.00m, "D"),  // -50  -> 1150
            (100.00m, "D")  // -100 -> 1050
        };

        // 2. Act (Executar)
        var result = BalanceCalculator.CalculateFinalBalance(initialBalance, movements);

        // 3. Assert (Verificar)
        result.Should().Be(1050.00m);
    }

    [Fact]
    public void CalculateFinalBalance_ShouldHandleZeroTransactions()
    {
        // Se não houver transações, o saldo final deve ser igual ao inicial
        var initialBalance = 500.00m;
        var movements = new List<(decimal Amount, string Type)>();

        var result = BalanceCalculator.CalculateFinalBalance(initialBalance, movements);

        result.Should().Be(500.00m);
    }

    [Fact]
    public void CalculateFinalBalance_ShouldHandleNegativeBalance()
    {
        var initialBalance = 10.00m;
        var movements = new List<(decimal Amount, string Type)>
        {
            (20.00m, "D") 
        };

        var result = BalanceCalculator.CalculateFinalBalance(initialBalance, movements);

        result.Should().Be(-10.00m);
    }

    [Fact]
    public void CalculateFinalBalance_ShouldThrowException_WhenTypeIsInvalid()
    {
        var movements = new List<(decimal Amount, string Type)> { (10.00m, "X") };

        // Verifica se o método lança uma exceção quando encontra um tipo desconhecido
        Action act = () => BalanceCalculator.CalculateFinalBalance(100.00m, movements);

        act.Should().Throw<ArgumentException>().WithMessage("*Invalid movement type*"); ;
    }

    [Fact]
    public void CalculateFinalBalance_ShouldHandleHighPrecisionDecimals()
    {
        // Arrange
        var initialBalance = 10.0000m;
        var movements = new List<(decimal Amount, string Type)>
        {
            (0.0001m, "C"),
            (0.0001m, "D")
        };

        // Act
        var result = BalanceCalculator.CalculateFinalBalance(initialBalance, movements);

        // Assert
        result.Should().Be(10.0000m);
    }

    [Fact]
    public void CalculateFinalBalance_ShouldNotModifyInputCollection()
    {
        // Arrange
        var movements = new List<(decimal Amount, string Type)> { (100.00m, "C") };
        var movementsCopy = new List<(decimal Amount, string Type)>(movements);

        // Act
        BalanceCalculator.CalculateFinalBalance(100.00m, movements);

        // Assert
        movements.Should().BeEquivalentTo(movementsCopy);
    }
}