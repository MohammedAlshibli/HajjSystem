using HajjSystem.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HajjSystem.Infrastructure.Data.Configurations;

public class FlightConfiguration : IEntityTypeConfiguration<Flight>
{
    public void Configure(EntityTypeBuilder<Flight> b)
    {
        b.HasKey(f => f.FlightId);

        // ParameterId stores business Value (34=Departure, 35=Return)
        b.HasOne(f => f.Parameter)
            .WithMany(p => p.Flights)
            .HasForeignKey(f => f.ParameterId)
            .HasPrincipalKey(p => p.Value)   // ← match on Value not PK
            .OnDelete(DeleteBehavior.Restrict);

        b.Property(f => f.FlightNo).IsRequired().HasMaxLength(20);
    }
}
