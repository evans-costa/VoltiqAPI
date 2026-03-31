namespace Voltiq.Domain.Interfaces.Repositories.Budget;

public interface IBudgetUpdateOnlyRepository
{
    Task<Entities.Budget?> GetByIdAndUserIdAsync(Guid id, Guid userId, CancellationToken cancellationToken = default);
    Task<Entities.Budget?> GetByIdWithItemsAndUserIdAsync(Guid id, Guid userId, CancellationToken cancellationToken = default);
    void Remove(Entities.Budget entity);
}
