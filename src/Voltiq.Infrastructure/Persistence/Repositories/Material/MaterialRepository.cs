using Microsoft.EntityFrameworkCore;
using Voltiq.Domain.Interfaces.Repositories.Material;

namespace Voltiq.Infrastructure.Persistence.Repositories.Material;

public sealed class MaterialRepository(ApplicationDbContext context)
    : Repository<Domain.Entities.Material>(context), IMaterialRepository
{
    public async Task<IReadOnlyList<Domain.Entities.Material>> GetByUserIdAsync(
        Guid userId, CancellationToken cancellationToken = default)
        => await Context.Materials
            .AsNoTracking()
            .Where(m => m.UserId == userId)
            .ToListAsync(cancellationToken);

    public async Task<Domain.Entities.Material?> GetByIdAndUserIdAsync(
        Guid id, Guid userId, CancellationToken cancellationToken = default)
        => await Context.Materials
            .AsNoTracking()
            .FirstOrDefaultAsync(m => m.Id == id && m.UserId == userId, cancellationToken);

    public async Task<IReadOnlyList<Domain.Entities.Material>> GetActiveByUserIdAsync(
        Guid userId, CancellationToken cancellationToken = default)
        => await Context.Materials
            .AsNoTracking()
            .Where(m => m.UserId == userId && m.IsActive)
            .ToListAsync(cancellationToken);
}
