using System.Security.Claims;
using System.Text;
using BookManagement.Core.Entities;
using Microsoft.IdentityModel.Tokens;

namespace BookManagement.Api.Helpers;

/// <summary>
/// Shared helper for generating JWT access tokens.
/// Used by both the API auth endpoints and the Web AccountEndpoints.
/// </summary>
public static class JwtTokenHelper
{
    public static string GenerateToken(
        ApplicationUser user,
        IEnumerable<string> roles,
        string jwtKey,
        string jwtIssuer,
        string jwtAudience,
        int expirationHours = 8)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id),
            new(ClaimTypes.Email, user.Email ?? string.Empty),
            new(ClaimTypes.Name, user.UserName ?? user.Email ?? string.Empty)
        };
        claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));

        var credentials = new SigningCredentials(
            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
            SecurityAlgorithms.HmacSha256);

        var token = new System.IdentityModel.Tokens.Jwt.JwtSecurityToken(
            issuer: jwtIssuer,
            audience: jwtAudience,
            claims: claims,
            expires: DateTime.UtcNow.AddHours(expirationHours),
            signingCredentials: credentials);

        return new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler().WriteToken(token);
    }
}
