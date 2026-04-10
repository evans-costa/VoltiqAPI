namespace Voltiq.Domain.Interfaces.Repositories.Budget;

public interface IBudgetUpdateOnlyRepository
{
    Task<Entities.Budget?> GetTrackedByIdAndUserIdAsync(Guid id, Guid userId,
        CancellationToken cancellationToken = default);

    Task<Entities.Budget?> GetTrackedByIdWithItemsAndUserIdAsync(Guid id, Guid userId,
        CancellationToken cancellationToken = default);

    void Remove(Entities.Budget entity);
}
