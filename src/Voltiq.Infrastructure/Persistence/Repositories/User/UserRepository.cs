using Microsoft.EntityFrameworkCore;
using Voltiq.Domain.Interfaces.Repositories.User;
using Voltiq.Domain.ValueObjects;

namespace Voltiq.Infrastructure.Persistence.Repositories.User;

public class UserRepository(ApplicationDbContext context)
    : Repository<Domain.Entities.User>(context), IUserRepository
{
    public async Task<bool> ExistsUserAsync(Document document, Email email,
        CancellationToken ct = default)
    {
        return await Context.Users.AsNoTracking().AnyAsync(user =>
                user.Document == document || user.Email == email,
            cancellationToken: ct);
    }

    public async Task<Domain.Entities.User?> GetByEmailAsync(Email email,
        CancellationToken ct = default)
    {
        return await Context.Users.AsNoTracking()
            .FirstOrDefaultAsync(user => user.Email == email, cancellationToken: ct);
    }
}
