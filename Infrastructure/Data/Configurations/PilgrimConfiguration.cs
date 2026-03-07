using HajjSystem.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HajjSystem.Infrastructure.Data.Configurations;

public class PilgrimConfiguration : IEntityTypeConfiguration<Pilgrim>
{
    public void Configure(EntityTypeBuilder<Pilgrim> b)
    {
        b.HasKey(p => p.PilgrimId);

        b.HasOne(p => p.Unit)
            .WithMany(u => u.Pilgrims)
            .HasForeignKey(p => p.UnitId)
            .OnDelete(DeleteBehavior.SetNull);

        b.HasOne(p => p.Type)
            .WithMany(pa => pa.Pilgrims)
            .HasForeignKey(p => p.TypeId)
            .OnDelete(DeleteBehavior.Restrict);

        b.HasOne(p => p.Document)
            .WithMany(d => d.Pilgrims)
            .HasForeignKey(p => p.DocumentId)
            .OnDelete(DeleteBehavior.SetNull);

        // Unique NIC per tenant per year (soft-deleted excluded)
        b.HasIndex(p => new { p.TenantId, p.NIC, p.HajjYear })
            .IsUnique()
            .HasFilter("[IsDeleted] = 0");

        b.Property(p => p.TenantId).IsRequired();
        b.Property(p => p.NIC).IsRequired().HasMaxLength(20);
        b.Property(p => p.FullName).IsRequired().HasMaxLength(200);
        b.Property(p => p.ServiceNumber).IsRequired().HasMaxLength(20);
    }
}
