namespace Voltiq.Domain.Interfaces.Repositories.RefreshToken;

public interface IRefreshTokenReadOnlyRepository
{
    Task<Entities.RefreshToken?> GetByTokenAsync(string token, CancellationToken cancellationToken = default);
}
