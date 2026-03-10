using Voltiq.Domain.ValueObjects;

namespace Voltiq.Domain.Interfaces.Repositories.User;

public interface IUserRepository : IRepository<Entities.User>
{
    Task<bool> ExistsUserAsync(Document document, Email email, CancellationToken ct =
        default);

    Task<Entities.User?> GetByEmailAsync(Email email, CancellationToken ct = default);
}
