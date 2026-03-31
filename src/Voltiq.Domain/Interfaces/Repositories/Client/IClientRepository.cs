using Voltiq.Domain.ValueObjects;

namespace Voltiq.Domain.Interfaces.Repositories.Client;

public interface IClientRepository : IRepository<Entities.Client>
{
    Task<IReadOnlyList<Entities.Client>> GetByUserIdAsync(Guid userId,
        CancellationToken cancellationToken = default);

    Task<Entities.Client?> GetByIdAndUserIdAsync(Guid id, Guid userId,
        CancellationToken cancellationToken = default);

    Task<bool> ExistsWithEmailForUserAsync(Email email, Guid userId, Guid? excludeId = null,
        CancellationToken cancellationToken = default);
}
