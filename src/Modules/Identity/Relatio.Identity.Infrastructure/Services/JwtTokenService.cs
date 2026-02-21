using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Relatio.Identity.Application.Interfaces;
using Relatio.Identity.Application.Models;
using Relatio.Identity.Domain.Models;
using Relatio.Identity.Infrastructure.Settings;

namespace Relatio.Identity.Infrastructure.Services;

public sealed class JwtTokenService : IJwtTokenService
{
    private static readonly JwtSecurityTokenHandler TokenHandler = new();
    private readonly JwtSettings _jwtSettings;

    public JwtTokenService(IOptions<JwtSettings> jwtSettings)
    {
        _jwtSettings = jwtSettings.Value;
    }

    public AccessTokenResult GenerateAccessToken(TokenClaims claims)
    {
        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.Secret));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        var expiresAt = DateTimeOffset.UtcNow.AddMinutes(_jwtSettings.ExpiryMinutes);

        var tokenClaims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, claims.UserId.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, claims.Email),
            new Claim(ClaimTypes.Role, claims.Role),
            new Claim(ClaimTypes.Name, claims.Username),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var token = new JwtSecurityToken(
            issuer: _jwtSettings.Issuer,
            audience: _jwtSettings.Audience,
            claims: tokenClaims,
            expires: expiresAt.UtcDateTime,
            signingCredentials: credentials);

        var tokenString = TokenHandler.WriteToken(token);

        return new AccessTokenResult(tokenString, expiresAt);
    }

    public RefreshToken GenerateRefreshToken(Guid userId)
    {
        var randomBytes = new byte[32];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomBytes);

        var tokenValue = Convert.ToBase64String(randomBytes);
        var expiresAt = DateTimeOffset.UtcNow.AddDays(_jwtSettings.RefreshTokenExpiryDays);

        return RefreshToken.Create(tokenValue, expiresAt, userId);
    }
}
