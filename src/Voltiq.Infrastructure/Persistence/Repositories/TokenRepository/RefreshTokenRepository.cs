using Microsoft.EntityFrameworkCore;
using Voltiq.Domain.Entities;
using Voltiq.Domain.Interfaces.Repositories.RefreshToken;

namespace Voltiq.Infrastructure.Persistence.Repositories.TokenRepository;

public sealed class RefreshTokenRepository(ApplicationDbContext context)
    : IRefreshTokenReadOnlyRepository, IRefreshTokenWriteOnlyRepository
{
    public async Task<RefreshToken?> GetByTokenAsync(string token, CancellationToken cancellationToken = default)
        => await context.RefreshTokens
            .FirstOrDefaultAsync(r => r.Token == token, cancellationToken);

    public async Task AddAsync(RefreshToken entity, CancellationToken cancellationToken = default)
        => await context.RefreshTokens.AddAsync(entity, cancellationToken);
}
