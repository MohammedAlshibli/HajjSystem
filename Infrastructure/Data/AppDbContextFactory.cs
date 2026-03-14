using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace HajjSystem.Infrastructure.Data;

/// <summary>
/// Used only by EF Core tooling (Add-Migration, Update-Database).
/// Never instantiated at runtime.
/// </summary>
public class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        // Walk up from Infrastructure/bin to find appsettings.json in Web project
        var basePath = Path.Combine(Directory.GetCurrentDirectory(), "..", "Web");

        var config = new ConfigurationBuilder()
            .SetBasePath(basePath)
            .AddJsonFile("appsettings.json", optional: false)
            .AddJsonFile("appsettings.Development.json", optional: true)
            .Build();

        var connectionString = config.GetConnectionString("DefaultConnection");

        var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
        optionsBuilder.UseSqlServer(connectionString,
            sql => sql.MigrationsAssembly(typeof(AppDbContext).Assembly.FullName));

        // Use SysAdmin defaults for migrations
        return new AppDbContext(optionsBuilder.Options,
            getTenantId: () => 0,
            isSysAdmin:  () => true);
    }
}
