using Billing.Application.Common.Abstractions;

namespace Billing.Api.Endpoints;

public static class AuthEndpoints
{
    public static IEndpointRouteBuilder MapAuthEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/auth").WithTags("Auth");

        group.MapPost("/login", (LoginRequest req, IJwtTokenService tokens) =>
        {
            // Demo credentials — replace with a real user store before production.
            if (req.Username != "demo" || req.Password != "demo")
                return Results.Unauthorized();

            var token = tokens.Issue(userId: req.Username, tenantId: req.TenantId);
            return Results.Ok(new { access_token = token.Token, expires_at = token.ExpiresAt });
        })
        .AllowAnonymous()
        .WithName("Login")
        .WithSummary("Exchange credentials for a JWT.");

        return app;
    }
}

public sealed record LoginRequest(string Username, string Password, Guid TenantId);
