using Billing.Application.Common.Abstractions;

using Microsoft.AspNetCore.Http;

namespace Billing.Infrastructure.Identity;

public sealed class HttpTenantProvider(IHttpContextAccessor accessor) : ITenantProvider
{
    public Guid TenantId
    {
        get
        {
            var claim = accessor.HttpContext?.User.FindFirst("tenant_id")?.Value;
            return Guid.TryParse(claim, out var id)
                ? id
                : throw new UnauthorizedAccessException("No tenant in the current context.");
        }
    }
}