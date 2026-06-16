using System.IdentityModel.Tokens.Jwt;
using System.Text;

using Billing.Infrastructure.Identity;

using FluentAssertions;

using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

using Moq;

namespace Billing.Tests.Infrastructure.Identity;

public class JwtTokenServiceTests
{
    private static readonly JwtOptions Options = new()
    {
        Issuer = "billing-api",
        Audience = "billing-client",
        SecretKey = "unit-test-secret-key-must-be-at-least-32-chars!",
        AccessTokenMinutes = 30
    };

    private static readonly DateTimeOffset FixedNow =
        new(2026, 1, 15, 12, 0, 0, TimeSpan.Zero);

    private static JwtTokenService CreateService()
    {
        var clock = new Mock<TimeProvider>();
        clock.Setup(c => c.GetUtcNow()).Returns(FixedNow);
        return new JwtTokenService(Microsoft.Extensions.Options.Options.Create(Options), clock.Object);
    }

    [Fact]
    public void Issue_ReturnsTokenAndExpiry()
    {
        var sut = CreateService();

        var result = sut.Issue("demo", Guid.NewGuid());

        result.Token.Should().NotBeNullOrWhiteSpace();
        result.ExpiresAt.Should().Be(FixedNow.AddMinutes(Options.AccessTokenMinutes));
    }

    [Fact]
    public void Issue_TokenCarriesSubAndTenantClaims()
    {
        var sut = CreateService();
        var tenantId = Guid.NewGuid();

        var result = sut.Issue("user-42", tenantId);

        var jwt = new JwtSecurityTokenHandler().ReadJwtToken(result.Token);
        jwt.Claims.Should().Contain(c => c.Type == JwtRegisteredClaimNames.Sub && c.Value == "user-42");
        jwt.Claims.Should().Contain(c => c.Type == "tenant_id" && c.Value == tenantId.ToString());
        jwt.Claims.Should().Contain(c => c.Type == JwtRegisteredClaimNames.Jti);
    }

    [Fact]
    public void Issue_TokenHasConfiguredIssuerAndAudience()
    {
        var sut = CreateService();

        var result = sut.Issue("demo", Guid.NewGuid());

        var jwt = new JwtSecurityTokenHandler().ReadJwtToken(result.Token);
        jwt.Issuer.Should().Be(Options.Issuer);
        jwt.Audiences.Should().ContainSingle().Which.Should().Be(Options.Audience);
    }

    [Fact]
    public void Issue_TokenValidatesAgainstConfiguredKey()
    {
        var sut = CreateService();

        var result = sut.Issue("demo", Guid.NewGuid());

        var handler = new JwtSecurityTokenHandler();
        var parameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = Options.Issuer,
            ValidateAudience = true,
            ValidAudience = Options.Audience,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Options.SecretKey)),
            ClockSkew = TimeSpan.Zero,
            LifetimeValidator = (notBefore, expires, _, _) =>
                FixedNow.UtcDateTime >= notBefore && FixedNow.UtcDateTime < expires
        };

        var act = () => handler.ValidateToken(result.Token, parameters, out _);

        act.Should().NotThrow();
    }

    [Fact]
    public void Issue_TwoTokensHaveDistinctJti()
    {
        var sut = CreateService();

        var a = sut.Issue("demo", Guid.NewGuid());
        var b = sut.Issue("demo", Guid.NewGuid());

        var jtiA = new JwtSecurityTokenHandler().ReadJwtToken(a.Token)
            .Claims.Single(c => c.Type == JwtRegisteredClaimNames.Jti).Value;
        var jtiB = new JwtSecurityTokenHandler().ReadJwtToken(b.Token)
            .Claims.Single(c => c.Type == JwtRegisteredClaimNames.Jti).Value;

        jtiA.Should().NotBe(jtiB);
    }
}
