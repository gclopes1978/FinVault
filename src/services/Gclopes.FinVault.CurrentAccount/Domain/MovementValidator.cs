namespace Gclopes.FinVault.CurrentAccount.Domain;

public class MovementValidator
{
    public static bool IsValid(decimal amount, string type)
        => amount > 0 && (type == "C" || type == "D");
}