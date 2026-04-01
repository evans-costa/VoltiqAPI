using Microsoft.EntityFrameworkCore;
using Shouldly;
using Voltiq.CommonTestUtilities.Builders;
using Voltiq.CommonTestUtilities.Database;
using Voltiq.CommonTestUtilities.Fixtures;
using Voltiq.Domain.ValueObjects;
using Voltiq.Infrastructure.Persistence;
using Voltiq.Infrastructure.Persistence.Repositories;
using Voltiq.Domain.Interfaces.Repositories.Client;
using Voltiq.Infrastructure.Persistence.Repositories.Client;
using Voltiq.Infrastructure.Persistence.Repositories.User;

namespace Voltiq.Infrastructure.Tests.Persistence;

public class ClientRepositoryTests(PostgreSqlContainerFixture fixture)
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

    [Fact]
    public async Task AddAndGetById_ShouldPersistClient()
    {
        var user = await TestDataBuilder.SeedUserAsync(_userRepository, _unitOfWork);
        var client = await TestDataBuilder.SeedClientAsync(_clientRepository, _unitOfWork, user.Id);

        var found =
            await _clientReadOnly.GetByIdAndUserIdAsync(client.Id, user.Id, TestContext.Current.CancellationToken);

        found.ShouldNotBeNull();
        found.Id.ShouldBe(client.Id);
        found.Name.ShouldBe("Cliente Teste");
        found.Phone.ShouldBe("(11) 99999-9999");
        found.Email.Value.ShouldBe("cliente@example.com");
        found.Address.Street.ShouldBe("Rua das Flores");
        found.Address.City.ShouldBe("São Paulo");
        found.UserId.ShouldBe(user.Id);
    }

    [Fact]
    public async Task GetByUserIdAsync_ShouldReturnOnlyClientsOfUser()
    {
        var user1 = await TestDataBuilder.SeedUserAsync(_userRepository, _unitOfWork);
        var user2 = await TestDataBuilder.SeedUserAsync(_userRepository, _unitOfWork,
            name: "Maria Santos", email: "maria@example.com", document: "11222333000181");

        await TestDataBuilder.SeedClientAsync(_clientRepository, _unitOfWork, user1.Id,
            name: "Cliente User1", email: "user1a@example.com");
        await TestDataBuilder.SeedClientAsync(_clientRepository, _unitOfWork, user1.Id,
            name: "Cliente User1 B", email: "user1b@example.com");
        await TestDataBuilder.SeedClientAsync(_clientRepository, _unitOfWork, user2.Id,
            name: "Cliente User2", email: "user2@example.com");

        var user1Clients =
            await _clientRepository.GetByUserIdAsync(user1.Id,
                TestContext.Current.CancellationToken);

        user1Clients.Count.ShouldBe(2);
        user1Clients.ShouldAllBe(c => c.UserId == user1.Id);
    }

    [Fact]
    public async Task GetByIdAndUserIdAsync_ShouldReturnClient_WhenBelongsToUser()
    {
        var user = await TestDataBuilder.SeedUserAsync(_userRepository, _unitOfWork);
        var client = await TestDataBuilder.SeedClientAsync(_clientRepository, _unitOfWork, user.Id);

        var found = await _clientReadOnly.GetByIdAndUserIdAsync(client.Id, user.Id,
            TestContext.Current.CancellationToken);

        found.ShouldNotBeNull();
        found.Id.ShouldBe(client.Id);
    }

    [Fact]
    public async Task GetByIdAndUserIdAsync_ShouldReturnNull_WhenClientBelongsToAnotherUser()
    {
        var user1 = await TestDataBuilder.SeedUserAsync(_userRepository, _unitOfWork);
        var user2 = await TestDataBuilder.SeedUserAsync(_userRepository, _unitOfWork,
            name: "Outro User", email: "outro@example.com", document: "11222333000181");

        var client = await TestDataBuilder.SeedClientAsync(_clientRepository, _unitOfWork, user1.Id);

        var found = await _clientReadOnly.GetByIdAndUserIdAsync(client.Id, user2.Id,
            TestContext.Current.CancellationToken);

        found.ShouldBeNull();
    }

    [Fact]
    public async Task ExistsWithEmailForUserAsync_ShouldReturnTrue_WhenEmailExistsForUser()
    {
        var user = await TestDataBuilder.SeedUserAsync(_userRepository, _unitOfWork);
        await TestDataBuilder.SeedClientAsync(_clientRepository, _unitOfWork, user.Id,
            email: "ocupado@example.com");

        var existingEmail = Email.Create("ocupado@example.com").Value;

        var exists = await _clientRepository.ExistsWithEmailForUserAsync(
            existingEmail, user.Id, cancellationToken: TestContext.Current.CancellationToken);

        exists.ShouldBeTrue();
    }

    [Fact]
    public async Task ExistsWithEmailForUserAsync_ShouldReturnFalse_WhenEmailDoesNotExistForUser()
    {
        var user = await TestDataBuilder.SeedUserAsync(_userRepository, _unitOfWork);
        var email = Email.Create("inexistente@example.co").Value;

        var exists = await _clientRepository.ExistsWithEmailForUserAsync(
            email, user.Id, cancellationToken: TestContext.Current.CancellationToken);

        exists.ShouldBeFalse();
    }

    [Fact]
    public async Task
        ExistsWithEmailForUserAsync_ShouldReturnFalse_WhenExcludeIdMatchesExistingClient()
    {
        var user = await TestDataBuilder.SeedUserAsync(_userRepository, _unitOfWork);
        var client = await TestDataBuilder.SeedClientAsync(_clientRepository, _unitOfWork, user.Id,
            email: "meu@example.com");

        var email = Email.Create("meu@example.com").Value;

        var exists = await _clientRepository.ExistsWithEmailForUserAsync(
            email, user.Id, client.Id, TestContext.Current.CancellationToken);

        exists.ShouldBeFalse();
    }

    [Fact]
    public async Task ExistsWithEmailForUserAsync_ShouldReturnFalse_WhenEmailBelongsToAnotherUser()
    {
        var user1 = await TestDataBuilder.SeedUserAsync(_userRepository, _unitOfWork);
        var user2 = await TestDataBuilder.SeedUserAsync(_userRepository, _unitOfWork,
            name: "Outro User", email: "outro@example.com", document: "11222333000181");

        await TestDataBuilder.SeedClientAsync(_clientRepository, _unitOfWork, user1.Id,
            email: "compartilhado@example.com");

        var clientEmail = Email.Create("compartilhado@example.com").Value;

        var exists = await _clientRepository.ExistsWithEmailForUserAsync(
            clientEmail, user2.Id, cancellationToken: TestContext.Current.CancellationToken);

        exists.ShouldBeFalse();
    }
}
