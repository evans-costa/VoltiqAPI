using Microsoft.EntityFrameworkCore;
using Voltiq.Domain.Interfaces.Repositories.Budget;

namespace Voltiq.Infrastructure.Persistence.Repositories.Budget;

public sealed class BudgetRepository(ApplicationDbContext context)
    : Repository<Domain.Entities.Budget>(context), IBudgetRepository
{
    public async Task<IReadOnlyList<Domain.Entities.Budget>> GetByUserIdAsync(
        Guid userId, CancellationToken cancellationToken = default)
        => await Context.Budgets
            .AsNoTracking()
            .Where(b => b.UserId == userId)
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<Domain.Entities.Budget>> GetByClientIdAsync(
        Guid clientId, CancellationToken cancellationToken = default)
        => await Context.Budgets
            .AsNoTracking()
            .Where(b => b.ClientId == clientId)
            .ToListAsync(cancellationToken);

    public async Task<Domain.Entities.Budget?> GetByIdAndUserIdAsync(
        Guid id, Guid userId, CancellationToken cancellationToken = default)
        => await Context.Budgets
            .AsNoTracking()
            .FirstOrDefaultAsync(b => b.Id == id && b.UserId == userId, cancellationToken);

    public async Task<Domain.Entities.Budget?> GetByIdWithItemsAsync(
        Guid id, CancellationToken cancellationToken = default)
        => await Context.Budgets
            .Include(b => b.Items)
            .FirstOrDefaultAsync(b => b.Id == id, cancellationToken);

    public async Task<Domain.Entities.Budget?> GetByIdWithItemsAndUserIdAsync(
        Guid id, Guid userId, CancellationToken cancellationToken = default)
        => await Context.Budgets
            .Include(b => b.Items)
            .FirstOrDefaultAsync(b => b.Id == id && b.UserId == userId, cancellationToken);
}
