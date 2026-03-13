using Microsoft.EntityFrameworkCore;
using Voltiq.Domain.Interfaces.Repositories.Client;
using Voltiq.Infrastructure.Persistence;
using Voltiq.Infrastructure.Persistence.Repositories;

namespace Voltiq.Infrastructure.Persistence.Repositories.Client;

public sealed class ClientRepository(ApplicationDbContext context)
    : Repository<Domain.Entities.Client>(context), IClientRepository
{
    public async Task<IReadOnlyList<Domain.Entities.Client>> GetByUserIdAsync(
        Guid userId, CancellationToken cancellationToken = default)
        => await Context.Clients
            .AsNoTracking()
            .Where(c => c.UserId == userId)
            .ToListAsync(cancellationToken);

    public async Task<Domain.Entities.Client?> GetByIdAndUserIdAsync(
        Guid id, Guid userId, CancellationToken cancellationToken = default)
        => await Context.Clients
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == id && c.UserId == userId, cancellationToken);
}
