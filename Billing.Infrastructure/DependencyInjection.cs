using Billing.Application.Common.Abstractions;
using Billing.Infrastructure.Identity;
using Billing.Infrastructure.Outbox;
using Billing.Infrastructure.Persistence;
using Billing.Infrastructure.Persistence.Interceptors;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Billing.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration cfg)
    {
       
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
