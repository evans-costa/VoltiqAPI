using Microsoft.EntityFrameworkCore;
using Voltiq.Domain.Entities;
using Voltiq.Domain.Interfaces.Repositories;
using Voltiq.Infrastructure.Persistence.Repositories;

namespace Voltiq.Infrastructure.Persistence.Repositories;

public class RefreshTokenRepository(ApplicationDbContext context)
    : Repository<RefreshToken>(context), IRefreshTokenRepository
{
    public async Task<RefreshToken?> GetByTokenAsync(string token, CancellationToken cancellationToken = default) =>
        await Context.RefreshTokens
            .FirstOrDefaultAsync(r => r.Token == token, cancellationToken);
}
