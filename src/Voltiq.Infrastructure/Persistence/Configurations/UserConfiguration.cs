using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Voltiq.Domain.Entities;

namespace Voltiq.Infrastructure.Persistence.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.HasKey(u => u.Id);

        builder.Property(u => u.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.OwnsOne(u => u.Email, email =>
        {
            email.Property(e => e.Value)
                .HasColumnName("Email")
                .IsRequired()
                .HasMaxLength(320);

            email.HasIndex(e => e.Value).IsUnique();
        });

        builder.OwnsOne(u => u.Document, doc =>
        {
            doc.Property(d => d.Value)
                .HasColumnName("Document")
                .IsRequired()
                .HasMaxLength(14);

            doc.HasIndex(d => d.Value).IsUnique();
        });

        builder.Property(u => u.PasswordHash)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(u => u.CreatedAt).IsRequired();
        builder.Property(u => u.CreatedBy).HasMaxLength(200);
        builder.Property(u => u.UpdatedAt);

        builder.Ignore(u => u.DomainEvents);
    }
}
