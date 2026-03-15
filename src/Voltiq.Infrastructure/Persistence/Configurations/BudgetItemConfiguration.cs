using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Voltiq.Domain.Entities;

namespace Voltiq.Infrastructure.Persistence.Configurations;

public sealed class BudgetItemConfiguration : IEntityTypeConfiguration<BudgetItem>
{
    public void Configure(EntityTypeBuilder<BudgetItem> builder)
    {
        builder.HasKey(i => i.Id);

        builder.Property(i => i.BudgetId).IsRequired();

        builder.Property(i => i.MaterialId);

        builder.Property(i => i.MaterialName)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(i => i.Unit)
            .HasConversion<int?>();

        builder.Property(i => i.Quantity).IsRequired();

        builder.Property(i => i.UnitPrice)
            .IsRequired()
            .HasPrecision(18, 2);

        builder.Property(i => i.TotalPrice)
            .IsRequired()
            .HasPrecision(18, 2);

        builder.HasOne<Material>()
            .WithMany()
            .HasForeignKey(i => i.MaterialId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasIndex(i => i.BudgetId)
            .HasDatabaseName("IX_BudgetItems_BudgetId");

        builder.HasIndex(i => i.MaterialId)
            .HasDatabaseName("IX_BudgetItems_MaterialId");

        builder.Ignore(i => i.DomainEvents);
    }
}
