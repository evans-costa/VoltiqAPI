namespace Voltiq.Domain.Interfaces.Repositories.Service;

public interface IServiceWriteOnlyRepository
{
    Task AddAsync(Entities.Service entity, CancellationToken cancellationToken = default);
}
