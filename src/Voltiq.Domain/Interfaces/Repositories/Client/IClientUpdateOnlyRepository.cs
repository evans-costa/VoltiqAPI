namespace Voltiq.Domain.Interfaces.Repositories.Client;

public interface IClientUpdateOnlyRepository
{
    Task<Entities.Client?> GetByIdAndUserIdAsync(Guid id, Guid userId, CancellationToken cancellationToken = default);
    void Remove(Entities.Client entity);
}
