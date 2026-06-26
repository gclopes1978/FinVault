using Gclopes.FinVault.Identity.Application.Commands;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace Gclopes.FinVault.Identity.Application.Handlers;

public class RegisterUserHandler(UserManager<IdentityUser> userManager) : IRequestHandler<RegisterUserCommand, bool>
{
    public async Task<bool> Handle(RegisterUserCommand request, CancellationToken cancellationToken)
    {
        var user = new IdentityUser { UserName = request.Email, Email = request.Email };
        var result = await userManager.CreateAsync(user, request.Password);

        // Just return success status
        return result.Succeeded;
    }
}