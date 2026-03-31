namespace Voltiq.Domain.Interfaces.Repositories.Material;

public interface IMaterialUpdateOnlyRepository
{
    Task<Entities.Material?> GetByIdAndUserIdAsync(Guid id, Guid userId, CancellationToken cancellationToken = default);
    void Remove(Entities.Material entity);
}
