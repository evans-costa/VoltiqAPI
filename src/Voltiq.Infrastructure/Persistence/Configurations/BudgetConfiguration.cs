using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Voltiq.Domain.Entities;

namespace Voltiq.Infrastructure.Persistence.Configurations;

public sealed class BudgetConfiguration : IEntityTypeConfiguration<Budget>
{
    public void Configure(EntityTypeBuilder<Budget> builder)
    {
        builder.HasKey(b => b.Id);

        builder.Property(b => b.UserId).IsRequired();

        builder.Property(b => b.ClientId).IsRequired();

        builder.Property(b => b.TotalAmount)
            .IsRequired()
            .HasPrecision(18, 2);

        builder.Property(b => b.Status)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(b => b.PdfUrl)
            .HasMaxLength(2048);

        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(b => b.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<Client>()
            .WithMany()
            .HasForeignKey(b => b.ClientId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(b => b.Items)
            .WithOne()
            .HasForeignKey(i => i.BudgetId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Property(b => b.CreatedAt).IsRequired();
        builder.Property(b => b.CreatedBy).HasMaxLength(200);
        builder.Property(b => b.UpdatedAt);

        builder.HasIndex(b => b.UserId)
            .HasDatabaseName("IX_Budgets_UserId");

        builder.HasIndex(b => b.ClientId)
            .HasDatabaseName("IX_Budgets_ClientId");

        builder.HasIndex(b => b.Status)
            .HasDatabaseName("IX_Budgets_Status");

        builder.Ignore(b => b.DomainEvents);
    }
}
