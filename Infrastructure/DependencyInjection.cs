using HajjSystem.Application.Common.Interfaces;
using HajjSystem.Application.Services.Interfaces;
using HajjSystem.Domain.Entities;
using HajjSystem.Domain.Entities.Identity;
using HajjSystem.Infrastructure.Data;
using HajjSystem.Infrastructure.Repositories;
using HajjSystem.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace HajjSystem.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration          cfg)
    {
        // ── Database ──────────────────────────────────────────────────────
        services.AddDbContext<AppDbContext>((sp, opts) =>
        {
            opts.UseSqlServer(
                cfg.GetConnectionString("DefaultConnection"),
                sql => sql.MigrationsAssembly(typeof(AppDbContext).Assembly.FullName));

            // Pass tenant resolver lambdas into DbContext constructor
            var current = sp.GetRequiredService<ICurrentUserService>();
            // NOTE: DbContext is registered with overloaded ctor via factory below
        });

        // Register DbContext via factory so we can pass Func<> delegates
        services.AddScoped<AppDbContext>(sp =>
        {
            var opts    = sp.GetRequiredService<DbContextOptions<AppDbContext>>();
            var current = sp.GetRequiredService<ICurrentUserService>();
            return new AppDbContext(opts,
                getTenantId: () => current.TenantId,
                isSysAdmin:  () => current.IsSysAdmin);
        });

        // ── Repositories ──────────────────────────────────────────────────
        services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        // ── Settings ──────────────────────────────────────────────────────
        services.AddSingleton<IHajjSettingsAccessor, HajjSettingsAccessor>();

        // ── External Services ─────────────────────────────────────────────
        services.AddHttpClient<IHrmsService, HrmsService>();

        return services;
    }
}
