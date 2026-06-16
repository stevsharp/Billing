namespace Billing.Application.Common.Abstractions;

public interface IJwtTokenService
{
    AccessToken Issue(string userId, Guid tenantId);
}

public sealed record AccessToken(string Token, DateTimeOffset ExpiresAt);
