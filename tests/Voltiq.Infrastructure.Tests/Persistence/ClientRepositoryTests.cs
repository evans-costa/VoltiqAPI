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

public class ClientRepositoryTests(PostgreSqlContainerFixture fixture)
    : IClassFixture<PostgreSqlContainerFixture>, IAsyncLifetime
{
    private static readonly Guid UserId = Guid.NewGuid();

    private ClientRepository _clientRepository = null!;

    private ApplicationDbContext _dbContext = null!;
    private UnitOfWork _unitOfWork = null!;
    private Repository<User> _userRepository = null!;

    public async ValueTask InitializeAsync()
    {
        _dbContext = ApplicationDbContextFactory.Create(fixture.Container.GetConnectionString(), UserId);
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
        var document = Document.Create("529.982.247-25").Value;
        return User.Register("João Silva", email, document, "$argon2id$hash");
    }

    private static Client MakeClient(Guid userId, string name = "Cliente Teste", string email = "cliente@example.com")
    {
        var emailVo = Email.Create(email).Value;
        return Client.Register(userId, name, "(11) 99999-9999", emailVo,
            Address.Create("Rua das Flores", "123", "São Paulo", "SP", "01310-100"));
    }

    private async Task<User> CreateAndSaveUserAsync()
    {
        var user = MakeUser();
        await _userRepository.AddAsync(user);
        await _unitOfWork.SaveChangesAsync();
        return user;
    }

    [Fact]
    public async Task AddAndGetById_ShouldPersistClient()
    {
        var user = await CreateAndSaveUserAsync();
        var client = MakeClient(user.Id);

        await _clientRepository.AddAsync(client, TestContext.Current.CancellationToken);
        await _unitOfWork.SaveChangesAsync(TestContext.Current.CancellationToken);

        var found =
            await _clientRepository.GetByIdAsync(client.Id, TestContext.Current.CancellationToken);

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
        var user1 = await CreateAndSaveUserAsync();

        var email2 = Email.Create("maria@example.com").Value;
        var doc2 = Document.Create("11222333000181").Value;
        var user2 = User.Register("Maria Santos", email2, doc2, "$argon2id$hash2");
        await _userRepository.AddAsync(user2, TestContext.Current.CancellationToken);
        await _unitOfWork.SaveChangesAsync(TestContext.Current.CancellationToken);

        var client1 = MakeClient(user1.Id, "Cliente User1", "user1a@example.com");
        var client2 = MakeClient(user1.Id, "Cliente User1 B", "user1b@example.com");
        var client3 = MakeClient(user2.Id, "Cliente User2", "user2@example.com");

        await _clientRepository.AddAsync(client1, TestContext.Current.CancellationToken);
        await _clientRepository.AddAsync(client2, TestContext.Current.CancellationToken);
        await _clientRepository.AddAsync(client3, TestContext.Current.CancellationToken);
        await _unitOfWork.SaveChangesAsync(TestContext.Current.CancellationToken);

        var user1Clients =
            await _clientRepository.GetByUserIdAsync(user1.Id,
                TestContext.Current.CancellationToken);

        user1Clients.Count.ShouldBe(2);
        user1Clients.ShouldAllBe(c => c.UserId == user1.Id);
    }

    [Fact]
    public async Task GetByIdAndUserIdAsync_ShouldReturnClient_WhenBelongsToUser()
    {
        var user = await CreateAndSaveUserAsync();
        var client = MakeClient(user.Id);

        await _clientRepository.AddAsync(client, TestContext.Current.CancellationToken);
        await _unitOfWork.SaveChangesAsync(TestContext.Current.CancellationToken);

        var found = await _clientRepository.GetByIdAndUserIdAsync(client.Id, user.Id,
            TestContext.Current.CancellationToken);

        found.ShouldNotBeNull();
        found.Id.ShouldBe(client.Id);
    }

    [Fact]
    public async Task GetByIdAndUserIdAsync_ShouldReturnNull_WhenClientBelongsToAnotherUser()
    {
        var user1 = await CreateAndSaveUserAsync();

        var email2 = Email.Create("outro@example.com").Value;
        var doc2 = Document.Create("11222333000181").Value;
        var user2 = User.Register("Outro User", email2, doc2, "$argon2id$hash2");
        await _userRepository.AddAsync(user2, TestContext.Current.CancellationToken);
        await _unitOfWork.SaveChangesAsync(TestContext.Current.CancellationToken);

        var client = MakeClient(user1.Id);
        await _clientRepository.AddAsync(client, TestContext.Current.CancellationToken);
        await _unitOfWork.SaveChangesAsync(TestContext.Current.CancellationToken);

        var found = await _clientRepository.GetByIdAndUserIdAsync(client.Id, user2.Id,
            TestContext.Current.CancellationToken);

        found.ShouldBeNull();
    }

    [Fact]
    public async Task ExistsWithEmailForUserAsync_ShouldReturnTrue_WhenEmailExistsForUser()
    {
        var user = await CreateAndSaveUserAsync();
        var client = MakeClient(user.Id, email: "ocupado@example.com");
        await _clientRepository.AddAsync(client, TestContext.Current.CancellationToken);
        await _unitOfWork.SaveChangesAsync(TestContext.Current.CancellationToken);

        var exists = await _clientRepository.ExistsWithEmailForUserAsync(
            "ocupado@example.com", user.Id, cancellationToken: TestContext.Current.CancellationToken);

        exists.ShouldBeTrue();
    }

    [Fact]
    public async Task ExistsWithEmailForUserAsync_ShouldReturnFalse_WhenEmailDoesNotExistForUser()
    {
        var user = await CreateAndSaveUserAsync();

        var exists = await _clientRepository.ExistsWithEmailForUserAsync(
            "inexistente@example.com", user.Id, cancellationToken: TestContext.Current.CancellationToken);

        exists.ShouldBeFalse();
    }

    [Fact]
    public async Task ExistsWithEmailForUserAsync_ShouldReturnFalse_WhenExcludeIdMatchesExistingClient()
    {
        var user = await CreateAndSaveUserAsync();
        var client = MakeClient(user.Id, email: "meu@example.com");
        await _clientRepository.AddAsync(client, TestContext.Current.CancellationToken);
        await _unitOfWork.SaveChangesAsync(TestContext.Current.CancellationToken);

        var exists = await _clientRepository.ExistsWithEmailForUserAsync(
            "meu@example.com", user.Id, client.Id, TestContext.Current.CancellationToken);

        exists.ShouldBeFalse();
    }

    [Fact]
    public async Task ExistsWithEmailForUserAsync_ShouldReturnFalse_WhenEmailBelongsToAnotherUser()
    {
        var user1 = await CreateAndSaveUserAsync();

        var email2 = Email.Create("outro@example.com").Value;
        var doc2 = Document.Create("11222333000181").Value;
        var user2 = User.Register("Outro User", email2, doc2, "$argon2id$hash2");
        await _userRepository.AddAsync(user2, TestContext.Current.CancellationToken);
        await _unitOfWork.SaveChangesAsync(TestContext.Current.CancellationToken);

        var client = MakeClient(user1.Id, email: "compartilhado@example.com");
        await _clientRepository.AddAsync(client, TestContext.Current.CancellationToken);
        await _unitOfWork.SaveChangesAsync(TestContext.Current.CancellationToken);

        var exists = await _clientRepository.ExistsWithEmailForUserAsync(
            "compartilhado@example.com", user2.Id, cancellationToken: TestContext.Current.CancellationToken);

        exists.ShouldBeFalse();
    }
}
