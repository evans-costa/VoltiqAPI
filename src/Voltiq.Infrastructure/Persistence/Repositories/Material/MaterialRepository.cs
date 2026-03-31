using Microsoft.EntityFrameworkCore;
using Voltiq.Domain.Interfaces.Repositories.Material;

namespace Voltiq.Infrastructure.Persistence.Repositories.Material;

public sealed class MaterialRepository(ApplicationDbContext context)
    : IMaterialReadOnlyRepository, IMaterialWriteOnlyRepository, IMaterialUpdateOnlyRepository
{
    public async Task<Domain.Entities.Material?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => await context.Materials
            .AsNoTracking()
            .FirstOrDefaultAsync(m => m.Id == id, cancellationToken);

    public async Task<IReadOnlyList<Domain.Entities.Material>> GetByUserIdAsync(
        Guid userId, CancellationToken cancellationToken = default)
        => await context.Materials
            .AsNoTracking()
            .Where(m => m.UserId == userId)
            .ToListAsync(cancellationToken);

    async Task<Domain.Entities.Material?> IMaterialReadOnlyRepository.GetByIdAndUserIdAsync(
        Guid id, Guid userId, CancellationToken cancellationToken)
        => await context.Materials
            .AsNoTracking()
            .FirstOrDefaultAsync(m => m.Id == id && m.UserId == userId, cancellationToken);

    async Task<Domain.Entities.Material?> IMaterialUpdateOnlyRepository.GetByIdAndUserIdAsync(
        Guid id, Guid userId, CancellationToken cancellationToken)
        => await context.Materials
            .FirstOrDefaultAsync(m => m.Id == id && m.UserId == userId, cancellationToken);

    public async Task<IReadOnlyList<Domain.Entities.Material>> GetActiveByUserIdAsync(
        Guid userId, CancellationToken cancellationToken = default)
        => await context.Materials
            .AsNoTracking()
            .Where(m => m.UserId == userId && m.IsActive)
            .ToListAsync(cancellationToken);

    public async Task AddAsync(Domain.Entities.Material entity, CancellationToken cancellationToken = default)
        => await context.Materials.AddAsync(entity, cancellationToken);

    public void Remove(Domain.Entities.Material entity)
        => context.Materials.Remove(entity);
}
