using HajjSystem.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HajjSystem.Infrastructure.Data.Configurations;

public class UnitConfiguration : IEntityTypeConfiguration<Unit>
{
    public void Configure(EntityTypeBuilder<Unit> b)
    {
        b.HasKey(u => u.UnitId);
        b.Property(u => u.UnitNameAr).IsRequired().HasMaxLength(200);
        b.Property(u => u.UnitNameEn).HasMaxLength(200);
        b.HasIndex(u => u.UnitCode).IsUnique();
    }
}
