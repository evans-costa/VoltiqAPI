namespace Voltiq.Domain.Interfaces.Repositories.Service;

public interface IServiceReadOnlyRepository
{
    Task<Entities.Service?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Entities.Service>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<Entities.Service?> GetByIdAndUserIdAsync(Guid id, Guid userId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Entities.Service>> GetActiveByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
}
