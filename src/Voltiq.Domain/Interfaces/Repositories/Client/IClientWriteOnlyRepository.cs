namespace Voltiq.Domain.Interfaces.Repositories.Client;

public interface IClientWriteOnlyRepository
{
    Task AddAsync(Entities.Client entity, CancellationToken cancellationToken = default);
}
