using Microsoft.EntityFrameworkCore;
using Shouldly;
using Voltiq.CommonTestUtilities.Database;
using Voltiq.CommonTestUtilities.Fixtures;
using Voltiq.Domain.Entities;
using Voltiq.Domain.ValueObjects;
using Voltiq.Infrastructure.Persistence;
using Voltiq.Infrastructure.Persistence.Repositories;
using Voltiq.Infrastructure.Persistence.Repositories.Client;

namespace Voltiq.Infrastructure.Tests.Persistence;

public class SoftDeleteTests(PostgreSqlContainerFixture fixture)
    : IClassFixture<PostgreSqlContainerFixture>, IAsyncLifetime
{
    private static readonly Guid UserId = Guid.NewGuid();

    private ClientRepository _clientRepository = null!;
    private ApplicationDbContext _dbContext = null!;
    private UnitOfWork _unitOfWork = null!;
    private Repository<User> _userRepository = null!;

    public async ValueTask InitializeAsync()
    {
        _dbContext =
            ApplicationDbContextFactory.Create(fixture.Container.GetConnectionString(), UserId);
        await _dbContext.Database.MigrateAsync();
        await DatabaseHelper.CleanAsync(_dbContext);

        _userRepository = new Repository<User>(_dbContext);
        _clientRepository = new ClientRepository(_dbContext);
        _unitOfWork = new UnitOfWork(_dbContext);
    }

    public async ValueTask DisposeAsync()
    {
        await _dbContext.DisposeAsync();
    }

    private static User MakeUser()
    {
        var email = Email.Create("joao@example.com").Value;
        var doc = Document.Create("529.982.247-25").Value;
        return User.Register("João Silva", email, doc, "$argon2id$hash");
    }

    private static Client MakeClient(Guid userId)
    {
        var email = Email.Create("cliente@example.com").Value;
        return Client.Register(userId, "Cliente Teste", "(11) 99999-9999", email,
            Address.Create("Rua das Flores", "123", "São Paulo", "SP", "01310-100"));
    }

    private async Task<(User user, Client client)> CreateUserAndClientAsync()
    {
        var user = MakeUser();
        await _userRepository.AddAsync(user);
        await _unitOfWork.SaveChangesAsync();

        var client = MakeClient(user.Id);
        await _clientRepository.AddAsync(client);
        await _unitOfWork.SaveChangesAsync();

        return (user, client);
    }

    [Fact]
    public async Task Remove_OnSoftDeletableEntity_ShouldNotPhysicallyDelete()
    {
        var (_, client) = await CreateUserAndClientAsync();

        _clientRepository.Remove(client);
        await _unitOfWork.SaveChangesAsync(TestContext.Current.CancellationToken);

        var count = await _dbContext.Clients
            .IgnoreQueryFilters()
            .CountAsync(c => c.Id == client.Id, TestContext.Current.CancellationToken);

        count.ShouldBe(1);
    }

    [Fact]
    public async Task Remove_ShouldSetIsDeletedAndDeletedAt()
    {
        var (_, client) = await CreateUserAndClientAsync();
        var before = DateTime.UtcNow;

        _clientRepository.Remove(client);
        await _unitOfWork.SaveChangesAsync(TestContext.Current.CancellationToken);

        var deleted = await _dbContext.Clients
            .IgnoreQueryFilters()
            .FirstAsync(c => c.Id == client.Id, TestContext.Current.CancellationToken);

        deleted.IsDeleted.ShouldBeTrue();
        deleted.DeletedAt.ShouldNotBeNull();
        deleted.DeletedAt!.Value.ShouldBeGreaterThanOrEqualTo(before);
    }

    [Fact]
    public async Task GlobalQueryFilter_ShouldExcludeDeletedEntitiesFromNormalQueries()
    {
        var (_, client) = await CreateUserAndClientAsync();

        _clientRepository.Remove(client);
        await _unitOfWork.SaveChangesAsync(TestContext.Current.CancellationToken);

        var found =
            await _clientRepository.GetByIdAsync(client.Id, TestContext.Current.CancellationToken);

        found.ShouldBeNull();
    }

    [Fact]
    public async Task GlobalQueryFilter_ShouldNotAffectNonDeletedEntities()
    {
        var (user, _) = await CreateUserAndClientAsync();

        var clients =
            await _clientRepository.GetByUserIdAsync(user.Id,
                TestContext.Current.CancellationToken);

        clients.Count.ShouldBe(1);
    }

    [Fact]
    public async Task IgnoreQueryFilters_ShouldReturnDeletedEntities()
    {
        var (_, client) = await CreateUserAndClientAsync();

        _clientRepository.Remove(client);
        await _unitOfWork.SaveChangesAsync(TestContext.Current.CancellationToken);

        var all = await _dbContext.Clients
            .IgnoreQueryFilters()
            .Where(c => c.Id == client.Id)
            .ToListAsync(TestContext.Current.CancellationToken);

        all.Count.ShouldBe(1);
        all[0].IsDeleted.ShouldBeTrue();
    }

    [Fact]
    public async Task Remove_ShouldAlsoSetUpdatedAt()
    {
        var (_, client) = await CreateUserAndClientAsync();
        var before = DateTime.UtcNow;

        _clientRepository.Remove(client);
        await _unitOfWork.SaveChangesAsync(TestContext.Current.CancellationToken);

        var deleted = await _dbContext.Clients
            .IgnoreQueryFilters()
            .FirstAsync(c => c.Id == client.Id, TestContext.Current.CancellationToken);

        deleted.UpdatedAt.ShouldNotBeNull();
        deleted.UpdatedAt!.Value.ShouldBeGreaterThanOrEqualTo(before);
    }
}
