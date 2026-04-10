namespace Voltiq.Domain.Interfaces.Repositories.Budget;

public interface IBudgetReadOnlyRepository
{
    Task<Entities.Budget?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Entities.Budget>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Entities.Budget>> GetByClientIdAsync(Guid clientId, CancellationToken cancellationToken = default);
    Task<Entities.Budget?> GetByIdAndUserIdAsync(Guid id, Guid userId, CancellationToken cancellationToken = default);
    Task<Entities.Budget?> GetByIdWithItemsAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Entities.Budget?> GetByIdWithItemsAndUserIdAsync(Guid id, Guid userId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Entities.Budget>> GetByUserIdWithClientAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<Entities.Budget?> GetByIdWithItemsAndClientAsync(Guid id, Guid userId, CancellationToken cancellationToken = default);
}
