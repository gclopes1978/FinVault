using FluentValidation;
using Gclopes.FinVault.PayrollAnalysis.Models;

namespace Gclopes.FinVault.PayrollAnalysis.Validators;

public class ImportPayrollRequestValidator : AbstractValidator<ImportPayrollRequest>
{
    public ImportPayrollRequestValidator()
    {
        RuleFor(x => x.SourceDocumentId)
            .NotEmpty().WithMessage("SourceDocumentId is required.");

        RuleFor(x => x.Data)
            .NotEmpty().WithMessage("At least one payroll line item is required.");

        RuleForEach(x => x.Data).SetValidator(new PayrollItemRequestValidator());
    }
}

public class PayrollItemRequestValidator : AbstractValidator<PayrollItemRequest>
{
    public PayrollItemRequestValidator()
    {
        RuleFor(x => x.Code)
            .GreaterThan(0).WithMessage("Code must be a positive rubric code.");

        RuleFor(x => x.Field)
            .NotEmpty().WithMessage("Field is required.")
            .MaximumLength(100).WithMessage("Field cannot exceed 100 characters.");

        RuleFor(x => x.Type)
            .NotEmpty().WithMessage("Type is required.")
            .Must(type => type is "abono" or "desconto")
            .WithMessage("Invalid type. Allowed values are 'abono' or 'desconto'.");

        // Unlike CurrentAccount's movements, amounts here can legitimately be negative
        // (descontos), so we only reject zero rather than requiring a positive value.
        RuleFor(x => x.Amount)
            .NotEqual(0m).WithMessage("Amount cannot be zero.");
    }
}
