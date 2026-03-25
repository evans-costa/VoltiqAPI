using Voltiq.Domain.Interfaces.Repositories;

namespace Voltiq.Domain.Interfaces.Repositories.Material;

public interface IMaterialRepository : IRepository<Entities.Material>
{
    Task<IReadOnlyList<Entities.Material>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);

    Task<Entities.Material?> GetByIdAndUserIdAsync(Guid id, Guid userId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Entities.Material>> GetActiveByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
}
