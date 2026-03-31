using Microsoft.EntityFrameworkCore;
using Voltiq.Domain.Interfaces.Repositories.User;
using Voltiq.Domain.ValueObjects;

namespace Voltiq.Infrastructure.Persistence.Repositories.User;

public sealed class UserRepository(ApplicationDbContext context)
    : IUserReadOnlyRepository, IUserWriteOnlyRepository
{
    public async Task<Domain.Entities.User?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => await context.Users.AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == id, cancellationToken);

    public async Task<bool> ExistsUserAsync(Document document, Email email,
        CancellationToken cancellationToken = default)
        => await context.Users.AsNoTracking()
            .AnyAsync(u => u.Document == document || u.Email == email, cancellationToken);

    public async Task<Domain.Entities.User?> GetByEmailAsync(Email email,
        CancellationToken cancellationToken = default)
        => await context.Users.AsNoTracking()
            .FirstOrDefaultAsync(u => u.Email == email, cancellationToken);

    public async Task AddAsync(Domain.Entities.User entity, CancellationToken cancellationToken = default)
        => await context.Users.AddAsync(entity, cancellationToken);
}
