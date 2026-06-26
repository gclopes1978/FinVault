using Gclopes.FinVault.Identity.Application.DTOs;
using MediatR;

namespace Gclopes.FinVault.Identity.Application.Commands;

public record LoginUserCommand(string Email, string Password) : IRequest<AuthenticationResponse?>;