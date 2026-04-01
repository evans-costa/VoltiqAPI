using Microsoft.EntityFrameworkCore;
using Voltiq.Domain.Interfaces.Repositories.Budget;

namespace Voltiq.Infrastructure.Persistence.Repositories.Budget;

public sealed class BudgetRepository(ApplicationDbContext context)
    : IBudgetReadOnlyRepository, IBudgetWriteOnlyRepository, IBudgetUpdateOnlyRepository
{
    public async Task<Domain.Entities.Budget?> GetByIdAsync(Guid id,
        CancellationToken cancellationToken = default)
    {
        return await context.Budgets
            .AsNoTracking()
            .FirstOrDefaultAsync(b => b.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyList<Domain.Entities.Budget>> GetByUserIdAsync(
        Guid userId, CancellationToken cancellationToken = default)
    {
        return await context.Budgets
            .AsNoTracking()
            .Where(b => b.UserId == userId)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Domain.Entities.Budget>> GetByClientIdAsync(
        Guid clientId, CancellationToken cancellationToken = default)
    {
        return await context.Budgets
            .AsNoTracking()
            .Where(b => b.ClientId == clientId)
            .ToListAsync(cancellationToken);
    }

    async Task<Domain.Entities.Budget?> IBudgetReadOnlyRepository.GetByIdAndUserIdAsync(
        Guid id, Guid userId, CancellationToken cancellationToken)
    {
        return await context.Budgets
            .AsNoTracking()
            .FirstOrDefaultAsync(b => b.Id == id && b.UserId == userId, cancellationToken);
    }

    public async Task<Domain.Entities.Budget?> GetByIdWithItemsAsync(
        Guid id, CancellationToken cancellationToken = default)
    {
        return await context.Budgets
            .Include(b => b.Items)
            .AsNoTracking()
            .FirstOrDefaultAsync(b => b.Id == id, cancellationToken);
    }

    async Task<Domain.Entities.Budget?> IBudgetReadOnlyRepository.GetByIdWithItemsAndUserIdAsync(
        Guid id, Guid userId, CancellationToken cancellationToken)
    {
        return await context.Budgets
            .Include(b => b.Items)
            .AsNoTracking()
            .FirstOrDefaultAsync(b => b.Id == id && b.UserId == userId, cancellationToken);
    }

    public void Remove(Domain.Entities.Budget entity)
    {
        context.Budgets.Remove(entity);
    }

    public async Task<Domain.Entities.Budget?> GetTrackedByIdAndUserIdAsync(
        Guid id, Guid userId, CancellationToken cancellationToken)
    {
        return await context.Budgets
            .FirstOrDefaultAsync(b => b.Id == id && b.UserId == userId, cancellationToken);
    }

    public async Task<Domain.Entities.Budget?> GetTrackedByIdWithItemsAndUserIdAsync(
        Guid id, Guid userId, CancellationToken cancellationToken)
    {
        return await context.Budgets
            .Include(b => b.Items)
            .FirstOrDefaultAsync(b => b.Id == id && b.UserId == userId, cancellationToken);
    }

    public async Task AddAsync(Domain.Entities.Budget entity,
        CancellationToken cancellationToken = default)
    {
        await context.Budgets.AddAsync(entity, cancellationToken);
    }

    public async Task<IReadOnlyList<Domain.Entities.Budget>> GetByUserIdWithClientAsync(
        Guid userId, CancellationToken cancellationToken = default)
    {
        return await context.Budgets
            .Include(b => b.Client)
            .AsNoTracking()
            .Where(b => b.UserId == userId)
            .ToListAsync(cancellationToken);
    }

    public async Task<Domain.Entities.Budget?> GetByIdWithItemsAndClientAsync(
        Guid id, Guid userId, CancellationToken cancellationToken = default)
    {
        return await context.Budgets
            .Include(b => b.Items)
            .Include(b => b.Client)
            .AsNoTracking()
            .FirstOrDefaultAsync(b => b.Id == id && b.UserId == userId, cancellationToken);
    }
}
