using Microsoft.EntityFrameworkCore;
using Voltiq.Domain.Interfaces.Repositories.Client;

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

    public async Task<bool> ExistsWithEmailForUserAsync(
        string email, Guid userId, Guid? excludeId = null, CancellationToken cancellationToken = default)
        => await Context.Clients
            .AsNoTracking()
            .AnyAsync(c => c.UserId == userId
                           && c.Email.Value == email
                           && (excludeId == null || c.Id != excludeId),
                cancellationToken);
}
