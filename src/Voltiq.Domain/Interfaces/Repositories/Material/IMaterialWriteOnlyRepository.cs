namespace Voltiq.Domain.Interfaces.Repositories.Material;

public interface IMaterialWriteOnlyRepository
{
    Task AddAsync(Entities.Material entity, CancellationToken cancellationToken = default);
}
