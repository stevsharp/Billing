using Billing.Application.Common.Abstractions;

using Microsoft.AspNetCore.Http;

namespace Billing.Infrastructure.Identity;

public sealed class HttpCurrentUser(IHttpContextAccessor accessor) : ICurrentUser
{
    public string? UserId =>
        accessor.HttpContext?.User.FindFirst("sub")?.Value
        ?? accessor.HttpContext?.User.Identity?.Name;
}
