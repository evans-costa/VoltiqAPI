using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Voltiq.Domain.Entities;
using Voltiq.Domain.Interfaces;

namespace Voltiq.Infrastructure.Persistence.Interceptors;

public sealed class SoftDeleteInterceptor : SaveChangesInterceptor
{
    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        if (eventData.Context is null)
            return base.SavingChangesAsync(
                eventData, result, cancellationToken);

        var entries =
            eventData
                .Context
                .ChangeTracker
                .Entries<ISoftDeletable>()
                .Where(e => e.State == EntityState.Deleted);

        foreach (var softDeletable in entries)
        {
            softDeletable.State = EntityState.Modified;
            softDeletable.Entity.SoftDelete();

            if (softDeletable.Entity is AuditableEntity auditable)
                auditable.UpdatedAt = DateTime.UtcNow;

            foreach (var owned in softDeletable.References
                         .Where(r => r.TargetEntry != null &&
                                     r.TargetEntry.Metadata.IsOwned() &&
                                     r.TargetEntry.State == EntityState.Deleted))
                owned.TargetEntry!.State = EntityState.Unchanged;
        }

        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }
}
