namespace Voltiq.Domain.Interfaces.Repositories.User;

public interface IUserWriteOnlyRepository
{
    Task AddAsync(Entities.User entity, CancellationToken cancellationToken = default);
}
