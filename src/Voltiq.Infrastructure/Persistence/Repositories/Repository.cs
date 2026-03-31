using Microsoft.EntityFrameworkCore;
using Voltiq.Domain.Entities;
using Voltiq.Domain.Interfaces.Repositories;

namespace Voltiq.Infrastructure.Persistence.Repositories;

public class Repository<T>(ApplicationDbContext context) : IRepository<T>
    where T : BaseEntity
{
    private readonly DbSet<T> _dbSet = context.Set<T>();
    protected ApplicationDbContext Context { get; } = context;

    public async Task<T?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbSet.FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
    }

    public async Task AddAsync(T entity, CancellationToken cancellationToken = default)
    {
        await _dbSet.AddAsync(entity, cancellationToken);
    }

    public void Update(T entity)
    {
        _dbSet.Update(entity);
    }

    public void Remove(T entity)
    {
        _dbSet.Remove(entity);
    }
}
