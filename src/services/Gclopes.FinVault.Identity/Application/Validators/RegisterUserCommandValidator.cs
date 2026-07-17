using FluentValidation;
using Gclopes.FinVault.Identity.Application.Commands;

namespace Gclopes.FinVault.Identity.Application.Validators;

public class RegisterUserCommandValidator : AbstractValidator<RegisterUserCommand>
{
    public RegisterUserCommandValidator()
    {
        RuleFor(x => x.FullName)
            .NotEmpty().WithMessage("Full name is required.")
            .MinimumLength(2).WithMessage("Full name must be at least 2 characters long.")
            .MaximumLength(100).WithMessage("Full name cannot exceed 100 characters.");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required.")
            .EmailAddress().WithMessage("A valid email is required.");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password is required.")
            .MinimumLength(8).WithMessage("Password must be at least 8 characters long.")
            .Matches("(?=.*[a-z])").WithMessage("Password must include at least one lowercase letter.")
            .Matches("(?=.*[A-Z])").WithMessage("Password must include at least one uppercase letter.")
            .Matches("(?=.*\\d)").WithMessage("Password must include at least one number.")
            .Matches("(?=.*[^A-Za-z0-9])").WithMessage("Password must include at least one special character.");

        RuleFor(x => x.ConfirmPassword)
            .NotEmpty().WithMessage("Confirm password is required.")
            .Equal(x => x.Password)
            .WithMessage("Passwords do not match.");
    }
}