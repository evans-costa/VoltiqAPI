using Microsoft.EntityFrameworkCore;
using Voltiq.Domain.Interfaces.Repositories.Client;
using Voltiq.Domain.ValueObjects;

namespace Voltiq.Infrastructure.Persistence.Repositories.Client;

public sealed class ClientRepository(ApplicationDbContext context)
    : IClientReadOnlyRepository, IClientWriteOnlyRepository, IClientUpdateOnlyRepository
{
    public async Task<IReadOnlyList<Domain.Entities.Client>> GetByUserIdAsync(
        Guid userId, CancellationToken cancellationToken = default)
        => await context.Clients
            .AsNoTracking()
            .Where(c => c.UserId == userId)
            .ToListAsync(cancellationToken);

    async Task<Domain.Entities.Client?> IClientReadOnlyRepository.GetByIdAndUserIdAsync(
        Guid id, Guid userId, CancellationToken cancellationToken)
        => await context.Clients
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == id && c.UserId == userId, cancellationToken);

    async Task<Domain.Entities.Client?> IClientUpdateOnlyRepository.GetByIdAndUserIdAsync(
        Guid id, Guid userId, CancellationToken cancellationToken)
        => await context.Clients
            .FirstOrDefaultAsync(c => c.Id == id && c.UserId == userId, cancellationToken);

    public async Task<bool> ExistsWithEmailForUserAsync(
        Email email, Guid userId, Guid? excludeId = null, CancellationToken cancellationToken = default)
        => await context.Clients
            .AsNoTracking()
            .AnyAsync(c => c.UserId == userId
                           && c.Email == email
                           && (excludeId == null || c.Id != excludeId),
                cancellationToken);

    public async Task AddAsync(Domain.Entities.Client entity, CancellationToken cancellationToken = default)
        => await context.Clients.AddAsync(entity, cancellationToken);

    public void Remove(Domain.Entities.Client entity)
        => context.Clients.Remove(entity);
}
