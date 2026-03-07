using HajjSystem.Domain.Entities.Identity;
using HajjSystem.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HajjSystem.Infrastructure.Data.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> b)
    {
        b.HasKey(u => u.UserId);
        b.Property(u => u.UserName).IsRequired().HasMaxLength(50);
        b.HasIndex(u => u.UserName).IsUnique();

        b.HasOne(u => u.Unit)
            .WithMany()
            .HasForeignKey(u => u.MainUnitId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}

public class RoleConfiguration : IEntityTypeConfiguration<Role>
{
    public void Configure(EntityTypeBuilder<Role> b)
    {
        b.HasKey(r => r.Id);
        b.Property(r => r.Name).IsRequired().HasMaxLength(100);
    }
}

public class UserRoleConfiguration : IEntityTypeConfiguration<UserRole>
{
    public void Configure(EntityTypeBuilder<UserRole> b)
    {
        b.HasKey(ur => new { ur.UserId, ur.RoleId });

        b.HasOne(ur => ur.User).WithMany(u => u.UserRoles).HasForeignKey(ur => ur.UserId);
        b.HasOne(ur => ur.Role).WithMany(r => r.UserRoles).HasForeignKey(ur => ur.RoleId);
    }
}

public class UserServiceConfiguration : IEntityTypeConfiguration<UserService>
{
    public void Configure(EntityTypeBuilder<UserService> b)
    {
        b.HasKey(us => new { us.UserId, us.ServiceId });

        b.HasOne(us => us.User).WithMany(u => u.UserServices).HasForeignKey(us => us.UserId);
        b.HasOne(us => us.Unit).WithMany(u => u.UserServices).HasForeignKey(us => us.ServiceId);
    }
}

public class PermissionConfiguration : IEntityTypeConfiguration<Permission>
{
    public void Configure(EntityTypeBuilder<Permission> b)
    {
        b.HasKey(p => p.PermissionId);
        b.Property(p => p.ControllerName).IsRequired().HasMaxLength(100);
        b.Property(p => p.ActionName).IsRequired().HasMaxLength(100);
    }
}

public class RolePermissionConfiguration : IEntityTypeConfiguration<RolePermission>
{
    public void Configure(EntityTypeBuilder<RolePermission> b)
    {
        b.HasKey(rp => new { rp.RoleId, rp.PermissionId });
        b.HasOne(rp => rp.Role).WithMany(r => r.RolePermissions).HasForeignKey(rp => rp.RoleId);
        b.HasOne(rp => rp.Permission).WithMany(p => p.RolePermissions).HasForeignKey(rp => rp.PermissionId);
    }
}
