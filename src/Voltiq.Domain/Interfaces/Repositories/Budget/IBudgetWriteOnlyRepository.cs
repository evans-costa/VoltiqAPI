namespace Voltiq.Domain.Interfaces.Repositories.Budget;

public interface IBudgetWriteOnlyRepository
{
    Task AddAsync(Entities.Budget entity, CancellationToken cancellationToken = default);
}
