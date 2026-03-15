using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Voltiq.Domain.Entities;

namespace Voltiq.Infrastructure.Persistence.Configurations;

public sealed class MaterialConfiguration : IEntityTypeConfiguration<Material>
{
    public void Configure(EntityTypeBuilder<Material> builder)
    {
        builder.HasKey(m => m.Id);

        builder.Property(m => m.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(m => m.DefaultPrice)
            .IsRequired()
            .HasPrecision(18, 2);

        builder.Property(m => m.Unit)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(m => m.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(m => m.CreatedAt).IsRequired();
        builder.Property(m => m.CreatedBy).HasMaxLength(200);
        builder.Property(m => m.UpdatedAt);

        builder.HasIndex(m => m.Name)
            .HasDatabaseName("IX_Materials_Name");

        builder.HasIndex(m => m.IsActive)
            .HasDatabaseName("IX_Materials_IsActive");

        builder.Ignore(m => m.DomainEvents);
    }
}
