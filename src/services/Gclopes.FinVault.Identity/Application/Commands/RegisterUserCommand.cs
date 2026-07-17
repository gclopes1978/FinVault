using MediatR;

namespace Gclopes.FinVault.Identity.Application.Commands;

public record RegisterUserCommand(string FullName, string Email, string Password, string ConfirmPassword) : IRequest<bool>;