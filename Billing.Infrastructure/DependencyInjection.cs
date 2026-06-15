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
        var secretKey = cfg["Jwt:SecretKey"]
            ?? throw new InvalidOperationException("Jwt:SecretKey is not configured.");

        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(opt =>
            {
                opt.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = cfg["Jwt:Issuer"],
                    ValidateAudience = true,
                    ValidAudience = cfg["Jwt:Audience"],
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)),
                    ClockSkew = TimeSpan.Zero
                };
            });

        services.AddSingleton(TimeProvider.System);

        services.AddScoped<ICurrentUser, HttpCurrentUser>();
        services.AddScoped<ITenantProvider, HttpTenantProvider>();
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
