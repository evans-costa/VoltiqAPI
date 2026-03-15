using Voltiq.Domain.Interfaces.Repositories;

namespace Voltiq.Domain.Interfaces.Repositories.Material;

public interface IMaterialRepository : IRepository<Entities.Material>
{
    Task<IReadOnlyList<Entities.Material>> GetActiveAsync(CancellationToken cancellationToken = default);
}
