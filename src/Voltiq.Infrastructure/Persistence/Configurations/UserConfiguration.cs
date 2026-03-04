using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Voltiq.Domain.Entities;
using Voltiq.Domain.ValueObjects;

namespace Voltiq.Infrastructure.Persistence.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.HasKey(u => u.Id);

        builder.Property(u => u.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(u => u.Email)
            .HasConversion(e => e.Value, v => Email.Create(v))
            .HasColumnName("Email")
            .IsRequired()
            .HasMaxLength(320);

        builder.HasIndex(u => u.Email).IsUnique();

        builder.Property(u => u.Document)
            .HasConversion(d => d.Value, v => Document.Create(v))
            .HasColumnName("Document")
            .IsRequired()
            .HasMaxLength(14);

        builder.HasIndex(u => u.Document).IsUnique();

        builder.Property(u => u.PasswordHash)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(u => u.CreatedAt).IsRequired();
        builder.Property(u => u.CreatedBy).HasMaxLength(200);
        builder.Property(u => u.UpdatedAt);

        builder.Ignore(u => u.DomainEvents);
    }
}
