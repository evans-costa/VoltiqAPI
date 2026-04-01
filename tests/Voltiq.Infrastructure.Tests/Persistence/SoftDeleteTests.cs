using Microsoft.EntityFrameworkCore;
using Shouldly;
using Voltiq.CommonTestUtilities.Builders;
using Voltiq.CommonTestUtilities.Database;
using Voltiq.CommonTestUtilities.Fixtures;
using Voltiq.Infrastructure.Persistence;
using Voltiq.Infrastructure.Persistence.Repositories;
using Voltiq.Domain.Interfaces.Repositories.Client;
using Voltiq.Infrastructure.Persistence.Repositories.Client;
using Voltiq.Infrastructure.Persistence.Repositories.User;

namespace Voltiq.Infrastructure.Tests.Persistence;

public class SoftDeleteTests(PostgreSqlContainerFixture fixture)
    : IClassFixture<PostgreSqlContainerFixture>, IAsyncLifetime
{
    private static readonly Guid UserId = Guid.NewGuid();

    private ClientRepository _clientRepository = null!;
    private IClientReadOnlyRepository _clientReadOnly = null!;
    private ApplicationDbContext _dbContext = null!;
    private UnitOfWork _unitOfWork = null!;
    private UserRepository _userRepository = null!;

    public async ValueTask InitializeAsync()
    {
        _dbContext =
            ApplicationDbContextFactory.Create(fixture.Container.GetConnectionString(), UserId);
        await _dbContext.Database.MigrateAsync();
        await DatabaseHelper.CleanAsync(_dbContext);

        _userRepository = new UserRepository(_dbContext);
        _clientRepository = new ClientRepository(_dbContext);
        _clientReadOnly = _clientRepository;
        _unitOfWork = new UnitOfWork(_dbContext);
    }

    public async ValueTask DisposeAsync()
    {
        await _dbContext.DisposeAsync();
    }

    private async Task<(Voltiq.Domain.Entities.User user, Voltiq.Domain.Entities.Client client)> CreateUserAndClientAsync()
    {
        var user = await TestDataBuilder.SeedUserAsync(_userRepository, _unitOfWork);
        var client = await TestDataBuilder.SeedClientAsync(_clientRepository, _unitOfWork, user.Id);
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
        var (user, client) = await CreateUserAndClientAsync();

        _clientRepository.Remove(client);
        await _unitOfWork.SaveChangesAsync(TestContext.Current.CancellationToken);

        var found =
            await _clientReadOnly.GetByIdAndUserIdAsync(client.Id, user.Id, TestContext.Current.CancellationToken);

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
