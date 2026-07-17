using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Gclopes.FinVault.Identity.Application.Commands;
using Gclopes.FinVault.Identity.Application.DTOs;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace Gclopes.FinVault.Identity.Application.Handlers;

public class LoginUserHandler(
    SignInManager<IdentityUser> signInManager,
    UserManager<IdentityUser> userManager,
    IConfiguration configuration) : IRequestHandler<LoginUserCommand, AuthenticationResponse?>
{
    public async Task<AuthenticationResponse?> Handle(LoginUserCommand request, CancellationToken cancellationToken)
    {
        // Attempt to sign in the user using the provided credentials
        var result = await signInManager.PasswordSignInAsync(request.Email, request.Password, false, false);

        if (!result.Succeeded)
            return null;

        // Retrieve the user from the database
        var user = await userManager.FindByEmailAsync(request.Email);
        if (user == null)
            return null;

        // Generate and return the JWT token
        var token = GenerateJwtToken(user);

        return new AuthenticationResponse(
            new JwtSecurityTokenHandler().WriteToken(token),
            token.ValidTo
        );
    }

    private JwtSecurityToken GenerateJwtToken(IdentityUser user)
    {
        var secretKey = configuration["JwtSettings:Secret"];

        // Ensure the JWT key is configured to prevent runtime exceptions
        if (string.IsNullOrEmpty(secretKey))
            throw new InvalidOperationException("JWT Secret Key is missing in configuration.");

        var authClaims = new List<Claim>
        {
            new(ClaimTypes.Name, user.Email!),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
        };

        var authSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));

        return new JwtSecurityToken(
            issuer: configuration["JwtSettings:Issuer"],
            audience: configuration["JwtSettings:Audience"],
            expires: DateTime.UtcNow.AddHours(2),
            claims: authClaims,
            signingCredentials: new SigningCredentials(authSigningKey, SecurityAlgorithms.HmacSha256)
        );
    }
}