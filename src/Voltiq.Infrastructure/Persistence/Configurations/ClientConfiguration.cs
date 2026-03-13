using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Voltiq.Domain.Entities;

namespace Voltiq.Infrastructure.Persistence.Configurations;

public sealed class ClientConfiguration : IEntityTypeConfiguration<Client>
{
    public void Configure(EntityTypeBuilder<Client> builder)
    {
        builder.HasKey(c => c.Id);

        builder.Property(c => c.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(c => c.Phone)
            .IsRequired()
            .HasMaxLength(20);

        builder.OwnsOne(c => c.Address, address =>
        {
            address.Property(a => a.Street)
                .HasColumnName("Street")
                .IsRequired()
                .HasMaxLength(200);

            address.Property(a => a.Number)
                .HasColumnName("Number")
                .IsRequired()
                .HasMaxLength(20);

            address.Property(a => a.City)
                .HasColumnName("City")
                .IsRequired()
                .HasMaxLength(100);

            address.Property(a => a.State)
                .HasColumnName("State")
                .IsRequired()
                .HasMaxLength(2);

            address.Property(a => a.ZipCode)
                .HasColumnName("ZipCode")
                .IsRequired()
                .HasMaxLength(20);
        });

        builder.Property(c => c.UserId).IsRequired();

        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(c => c.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Property(c => c.CreatedAt).IsRequired();
        builder.Property(c => c.CreatedBy).HasMaxLength(200);
        builder.Property(c => c.UpdatedAt);

        builder.Ignore(c => c.DomainEvents);
    }
}
