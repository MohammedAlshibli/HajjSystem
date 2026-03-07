using HajjSystem.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HajjSystem.Infrastructure.Data.Configurations;

public class FlightConfiguration : IEntityTypeConfiguration<Flight>
{
    public void Configure(EntityTypeBuilder<Flight> b)
    {
        b.HasKey(f => f.FlightId);

        b.HasOne(f => f.Parameter)
            .WithMany(p => p.Flights)
            .HasForeignKey(f => f.ParameterId)
            .OnDelete(DeleteBehavior.Restrict);

        b.Property(f => f.FlightNo).IsRequired().HasMaxLength(20);
    }
}
