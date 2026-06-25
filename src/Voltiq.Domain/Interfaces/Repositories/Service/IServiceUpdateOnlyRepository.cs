namespace Voltiq.Domain.Interfaces.Repositories.Service;

public interface IServiceUpdateOnlyRepository
{
    Task<Entities.Service?> GetTrackedByIdAndUserIdAsync(Guid id, Guid userId, CancellationToken cancellationToken = default);
    void Remove(Entities.Service entity);
}
