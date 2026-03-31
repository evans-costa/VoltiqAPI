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

        builder.Property(m => m.UserId).IsRequired();

        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(m => m.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Property(m => m.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(m => m.CreatedAt).IsRequired();
        builder.Property(m => m.CreatedBy).HasMaxLength(200);
        builder.Property(m => m.UpdatedAt);

        builder.Property(m => m.IsDeleted).IsRequired().HasDefaultValue(false);
        builder.Property(m => m.DeletedAt);

        builder.HasIndex(m => m.Name)
            .HasDatabaseName("IX_Materials_Name");

        builder.HasIndex(m => m.UserId)
            .HasDatabaseName("IX_Materials_UserId");

        builder.HasIndex(m => m.IsActive)
            .HasDatabaseName("IX_Materials_IsActive");

        builder.HasIndex(m => m.IsDeleted)
            .HasDatabaseName("IX_Materials_IsDeleted");

        builder.HasQueryFilter(m => !m.IsDeleted);

        builder.Ignore(m => m.DomainEvents);
    }
}
