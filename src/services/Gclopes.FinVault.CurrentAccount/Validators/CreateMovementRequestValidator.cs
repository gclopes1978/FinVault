using FluentValidation;
using Gclopes.FinVault.CurrentAccount.Models;

namespace Gclopes.FinVault.CurrentAccount.Validators;

public class CreateMovementRequestValidator : AbstractValidator<CreateMovementRequest>
{
    public CreateMovementRequestValidator()
    {
        // Amount must be strictly positive (greater than zero)
        RuleFor(x => x.Amount)
            .GreaterThan(0).WithMessage("Amount must be greater than zero.");

        // Type must be either 'Credit' or 'Debit'
        RuleFor(x => x.Type)
            .NotEmpty().WithMessage("Movement type is required.")
            .Must(type => type == "Credit" || type == "Debit")
            .WithMessage("Invalid movement type. Allowed values are 'Credit' or 'Debit'.");

        // Description cannot be empty
        RuleFor(x => x.Description)
            .NotEmpty().WithMessage("Description is required.")
            .MaximumLength(100).WithMessage("Description cannot exceed 100 characters.");

        // Category cannot be empty
        RuleFor(x => x.Category)
            .NotEmpty().WithMessage("Category is required.")
            .MaximumLength(50).WithMessage("Category cannot exceed 50 characters.");

        // TransactionDate must be a valid date (not default)
        RuleFor(x => x.TransactionDate)
            .NotEmpty().WithMessage("Transaction date is required.");

        // ValueDate must be a valid date (not default)
        RuleFor(x => x.ValueDate)
            .NotEmpty().WithMessage("Value date is required.");
    }
}