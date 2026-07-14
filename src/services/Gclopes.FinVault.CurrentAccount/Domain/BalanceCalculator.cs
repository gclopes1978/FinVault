namespace Gclopes.FinVault.CurrentAccount.Domain;

public static class BalanceCalculator
{
    // Returns the final balance based on an initial amount and a list of movements
    public static decimal CalculateFinalBalance(decimal initialBalance, IEnumerable<(decimal Amount, string Type)> movements)
    {
        decimal balance = initialBalance;
        foreach (var movement in movements)
        {
            if (movement.Type == "C")
                balance += movement.Amount;
            else if (movement.Type == "D")
                balance -= movement.Amount;
            else
                // Aqui lançamos a exceção que o teste espera
                throw new ArgumentException($"Invalid movement type: {movement.Type}. Expected 'C' or 'D'.");
        }
        return balance;
    }
}