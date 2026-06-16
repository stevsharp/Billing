using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

using Billing.Application.Common.Abstractions;

using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace Billing.Infrastructure.Identity;

public sealed class JwtTokenService(IOptions<JwtOptions> options, TimeProvider clock) : IJwtTokenService
{
    private readonly JwtOptions _opt = options.Value;

    public AccessToken Issue(string userId, Guid tenantId)
    {
        var now = clock.GetUtcNow();
        var expires = now.AddMinutes(_opt.AccessTokenMinutes);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, userId),
            new Claim("tenant_id", tenantId.ToString()),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_opt.SecretKey));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var jwt = new JwtSecurityToken(
            issuer: _opt.Issuer,
            audience: _opt.Audience,
            claims: claims,
            notBefore: now.UtcDateTime,
            expires: expires.UtcDateTime,
            signingCredentials: creds);

        var token = new JwtSecurityTokenHandler().WriteToken(jwt);
        return new AccessToken(token, expires);
    }
}
