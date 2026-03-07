using HajjSystem.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HajjSystem.Infrastructure.Data.Configurations;

public class PassengerConfiguration : IEntityTypeConfiguration<Passenger>
{
    public void Configure(EntityTypeBuilder<Passenger> b)
    {
        b.HasKey(p => p.PassengerId);

        b.HasOne(p => p.Flight)
            .WithMany(f => f.Passengers)
            .HasForeignKey(p => p.FlightId)
            .OnDelete(DeleteBehavior.Cascade);

        b.HasOne(p => p.Pilgrim)
            .WithMany(m => m.Passengers)
            .HasForeignKey(p => p.PilgrimId)
            .OnDelete(DeleteBehavior.Cascade);

        b.HasOne(p => p.Bus)
            .WithMany(bus => bus.Passengers)
            .HasForeignKey(p => p.BusId)
            .OnDelete(DeleteBehavior.Restrict);

        b.HasOne(p => p.Residence)
            .WithMany(r => r.Passengers)
            .HasForeignKey(p => p.ResidenceId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
