namespace Voltiq.Domain.Interfaces.Repositories.RefreshToken;

public interface IRefreshTokenWriteOnlyRepository
{
    Task AddAsync(Entities.RefreshToken entity, CancellationToken cancellationToken = default);
}
