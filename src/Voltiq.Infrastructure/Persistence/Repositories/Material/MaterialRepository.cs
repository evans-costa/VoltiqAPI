using Microsoft.EntityFrameworkCore;
using Voltiq.Domain.Interfaces.Repositories.Material;

namespace Voltiq.Infrastructure.Persistence.Repositories.Material;

public sealed class MaterialRepository(ApplicationDbContext context)
    : Repository<Domain.Entities.Material>(context), IMaterialRepository
{
    public async Task<IReadOnlyList<Domain.Entities.Material>> GetActiveAsync(
        CancellationToken cancellationToken = default)
        => await Context.Materials
            .AsNoTracking()
            .Where(m => m.IsActive)
            .ToListAsync(cancellationToken);
}
