using Voltiq.Domain.Interfaces.Repositories;

namespace Voltiq.Domain.Interfaces.Repositories.Budget;

public interface IBudgetRepository : IRepository<Entities.Budget>
{
    Task<IReadOnlyList<Entities.Budget>> GetByClientIdAsync(Guid clientId, CancellationToken cancellationToken = default);

    Task<Entities.Budget?> GetByIdWithItemsAsync(Guid id, CancellationToken cancellationToken = default);
}
