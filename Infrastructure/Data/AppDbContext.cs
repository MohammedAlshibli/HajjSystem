using HajjSystem.Domain.Entities;
using HajjSystem.Domain.Entities.Identity;
using Microsoft.EntityFrameworkCore;

namespace HajjSystem.Infrastructure.Data;

public class AppDbContext : DbContext
{
    private readonly Func<int>  _getTenantId;
    private readonly Func<bool> _isSysAdmin;

    public AppDbContext(DbContextOptions<AppDbContext> options,
                        Func<int>  getTenantId,
                        Func<bool> isSysAdmin) : base(options)
    {
        _getTenantId = getTenantId ?? (() => 0);
        _isSysAdmin  = isSysAdmin  ?? (() => true);
    }

    // Parameterless constructor for migrations / tooling
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
        _getTenantId = () => 0;
        _isSysAdmin  = () => true;
    }

    // ── DbSets ────────────────────────────────────────────────────────
    public DbSet<Pilgrim>    Pilgrims    { get; set; }
    public DbSet<Unit>       Units       { get; set; }
    public DbSet<Parameter>  Parameters  { get; set; }
    public DbSet<Flight>     Flights     { get; set; }
    public DbSet<Passenger>  Passengers  { get; set; }
    public DbSet<Bus>        Buses       { get; set; }
    public DbSet<Residence>  Residences  { get; set; }
    public DbSet<Document>   Documents   { get; set; }

    public DbSet<User>           Users       { get; set; }
    public DbSet<Role>           Roles       { get; set; }
    public DbSet<UserRole>       UserRoles   { get; set; }
    public DbSet<UserService>    UserServices{ get; set; }
    public DbSet<Permission>     Permissions { get; set; }
    public DbSet<RolePermission> RolePermissions { get; set; }

    protected override void OnModelCreating(ModelBuilder mb)
    {
        mb.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);

        // ── Global Query Filters ─────────────────────────────────────────
        mb.Entity<Pilgrim>().HasQueryFilter(p =>
            (_isSysAdmin() || p.TenantId == _getTenantId()) && !p.IsDeleted);

        mb.Entity<Passenger>().HasQueryFilter(p =>
            (_isSysAdmin() || p.TenantId == _getTenantId()) && !p.IsDeleted);

        mb.Entity<User>().HasQueryFilter(u =>
            _isSysAdmin() || u.TenantId == _getTenantId());
    }
}
