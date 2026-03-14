using HajjSystem.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HajjSystem.Infrastructure.Data.Configurations;

public class DocumentConfiguration : IEntityTypeConfiguration<Document>
{
    public void Configure(EntityTypeBuilder<Document> b)
    {
        b.HasKey(d => d.DocumentId);

        b.Property(d => d.FileName).IsRequired().HasMaxLength(500);
        b.Property(d => d.ContentType).HasMaxLength(100);
        b.Property(d => d.Path).HasMaxLength(1000);
        b.Property(d => d.DocumentType).HasMaxLength(50);
    }
}
