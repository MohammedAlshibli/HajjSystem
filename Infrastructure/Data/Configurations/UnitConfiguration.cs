using HajjSystem.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HajjSystem.Infrastructure.Data.Configurations;

public class UnitConfiguration : IEntityTypeConfiguration<Unit>
{
    public void Configure(EntityTypeBuilder<Unit> b)
    {
        b.ToTable("units");                          // lowercase table name

        b.HasKey(u => u.Id);
        b.Property(u => u.Id).HasColumnName("id");

        b.Property(u => u.UnitCode).HasColumnName("unit_code");
        b.Property(u => u.UnitNameAr).HasColumnName("unit_name_ar").IsRequired().HasMaxLength(200);
        b.Property(u => u.UnitNameEn).HasColumnName("unit_name_en").HasMaxLength(200);
        b.Property(u => u.AllowNumber).HasColumnName("allow_number");
        b.Property(u => u.StandBy).HasColumnName("stand_by");
        b.Property(u => u.UnitOrder).HasColumnName("unit_order");
        b.Property(u => u.ModFlag).HasColumnName("mod_flag");
        b.Property(u => u.HajjYear).HasColumnName("hajj_year");

        b.HasIndex(u => u.UnitCode).IsUnique();
    }
}
