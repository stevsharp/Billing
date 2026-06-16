using System.Text;

using Billing.Application.Common.Abstractions;
using Billing.Infrastructure.Identity;
using Billing.Infrastructure.Outbox;
using Billing.Infrastructure.Persistence;
using Billing.Infrastructure.Persistence.Interceptors;

using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;

namespace Billing.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration cfg)
    {
        services.AddOptions<JwtOptions>()
            .Bind(cfg.GetSection(JwtOptions.SectionName))
            .Validate(o => !string.IsNullOrWhiteSpace(o.SecretKey) && o.SecretKey.Length >= 32,
                "Jwt:SecretKey must be configured and at least 32 characters.")
            .ValidateOnStart();

        var jwt = cfg.GetSection(JwtOptions.SectionName).Get<JwtOptions>()
            ?? throw new InvalidOperationException("Jwt section is not configured.");

        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(opt =>
            {
                opt.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = jwt.Issuer,
                    ValidateAudience = true,
                    ValidAudience = jwt.Audience,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.SecretKey)),
                    ClockSkew = TimeSpan.Zero
                };
            });

        services.AddSingleton(TimeProvider.System);

        services.AddScoped<ICurrentUser, HttpCurrentUser>();
        services.AddScoped<ITenantProvider, HttpTenantProvider>();
        services.AddScoped<IJwtTokenService, JwtTokenService>();
        services.AddScoped<DomainInterceptor>();

        services.AddDbContext<BillingWriteContext>((sp, opt) =>
            opt.UseSqlServer(cfg.GetConnectionString("Primary"),
                    sql => sql.EnableRetryOnFailure())
               .AddInterceptors(sp.GetRequiredService<DomainInterceptor>()));

        services.AddDbContext<BillingReadContext>((sp, opt) =>
            opt.UseSqlServer(cfg.GetConnectionString("ReadReplica") ?? cfg.GetConnectionString("Primary"))
               .UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking));


        services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<BillingWriteContext>());
        services.AddScoped<IBillingDbContext>(sp => sp.GetRequiredService<BillingWriteContext>());
        services.AddScoped<IReadDbContext>(sp => sp.GetRequiredService<BillingReadContext>());

        services.AddHostedService<OutboxPublisher>();

        return services;
    }
}
