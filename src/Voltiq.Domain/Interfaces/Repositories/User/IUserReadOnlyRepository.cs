using Voltiq.Domain.ValueObjects;

namespace Voltiq.Domain.Interfaces.Repositories.User;

public interface IUserReadOnlyRepository
{
    Task<Entities.User?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<bool> ExistsUserAsync(Document document, Email email, CancellationToken cancellationToken = default);
    Task<Entities.User?> GetByEmailAsync(Email email, CancellationToken cancellationToken = default);
}
