using Gclopes.FinVault.Identity.Application.Commands;
using MediatR;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;

namespace Gclopes.FinVault.Identity.Application.Handlers;

public class RegisterUserHandler(UserManager<IdentityUser> userManager) : IRequestHandler<RegisterUserCommand, bool>
{
    public async Task<bool> Handle(RegisterUserCommand request, CancellationToken cancellationToken)
    {
        var user = new IdentityUser { UserName = request.Email, Email = request.Email };
        var result = await userManager.CreateAsync(user, request.Password);

        if (!result.Succeeded)
        {
            return false;
        }

        var claimResult = await userManager.AddClaimAsync(user, new Claim("fullName", request.FullName));

        return claimResult.Succeeded;
    }
}