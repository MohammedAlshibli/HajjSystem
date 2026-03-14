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
            .HasPrincipalKey(u => u.Id)
            .OnDelete(DeleteBehavior.SetNull);

        // TypeId stores the business Value (1=Regular,2=StandBy,3=Admin)
        // FK references Parameter.Value via shadow property — or just drop FK
        // and resolve in-memory using HajjConstants. This is cleaner.
        b.HasOne(p => p.Type)
            .WithMany(pa => pa.Pilgrims)
            .HasForeignKey(p => p.TypeId)
            .HasPrincipalKey(pa => pa.Value)   // ← match on Value not PK
            .OnDelete(DeleteBehavior.Restrict);

        b.HasOne(p => p.Document)
            .WithMany(d => d.Pilgrims)
            .HasForeignKey(p => p.DocumentId)
            .OnDelete(DeleteBehavior.SetNull);

        b.HasIndex(p => new { p.TenantId, p.NIC, p.HajjYear })
            .IsUnique()
            .HasFilter("[IsDeleted] = 0");

        b.Property(p => p.TenantId).IsRequired();
        b.Property(p => p.NIC).IsRequired().HasMaxLength(20);
        b.Property(p => p.FullName).IsRequired().HasMaxLength(200);
        b.Property(p => p.ServiceNumber).IsRequired().HasMaxLength(20);
    }
}
