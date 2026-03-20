using Microsoft.EntityFrameworkCore;
using Moq;
using Shouldly;
using Testcontainers.PostgreSql;
using Voltiq.Application.Common.Interfaces;
using Voltiq.Domain.Entities;
using Voltiq.Domain.ValueObjects;
using Voltiq.Infrastructure.Persistence;
using Voltiq.Infrastructure.Persistence.Repositories;
using Voltiq.Infrastructure.Persistence.Repositories.Client;

namespace Voltiq.Infrastructure.Tests.Persistence;

public class ClientRepositoryTests : IAsyncLifetime
{
    private static readonly Guid UserId = Guid.NewGuid();

    private readonly PostgreSqlContainer _postgres = new PostgreSqlBuilder("postgres:16-alpine")
        .Build();

    private ClientRepository _clientRepository = null!;

    private ApplicationDbContext _dbContext = null!;
    private UnitOfWork _unitOfWork = null!;
    private Repository<User> _userRepository = null!;

    public async Task InitializeAsync()
    {
        await _postgres.StartAsync();

        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseNpgsql(_postgres.GetConnectionString())
            .Options;

        var currentUser = new Mock<ICurrentUserService>();
        currentUser.Setup(s => s.UserId).Returns(UserId);

        _dbContext = new ApplicationDbContext(options, currentUser.Object);
        await _dbContext.Database.MigrateAsync();

        _userRepository = new Repository<User>(_dbContext);
        _clientRepository = new ClientRepository(_dbContext);
        _unitOfWork = new UnitOfWork(_dbContext);
    }

    public async Task DisposeAsync()
    {
        await _dbContext.DisposeAsync();
        await _postgres.DisposeAsync();
    }

    private static User MakeUser()
    {
        var email = Email.Create("joao@example.com").Value;
        var document = Document.Create("529.982.247-25").Value;
        return User.Register("João Silva", email, document, "$argon2id$hash");
    }

    private static Client MakeClient(Guid userId, string name = "Cliente Teste")
    {
        return Client.Register(userId, name, "(11) 99999-9999",
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

        await _clientRepository.AddAsync(client);
        await _unitOfWork.SaveChangesAsync();

        var found = await _clientRepository.GetByIdAsync(client.Id);

        found.ShouldNotBeNull();
        found!.Id.ShouldBe(client.Id);
        found.Name.ShouldBe("Cliente Teste");
        found.Phone.ShouldBe("(11) 99999-9999");
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
        await _userRepository.AddAsync(user2);
        await _unitOfWork.SaveChangesAsync();

        var client1 = MakeClient(user1.Id, "Cliente User1");
        var client2 = MakeClient(user1.Id, "Cliente User1 B");
        var client3 = MakeClient(user2.Id, "Cliente User2");

        await _clientRepository.AddAsync(client1);
        await _clientRepository.AddAsync(client2);
        await _clientRepository.AddAsync(client3);
        await _unitOfWork.SaveChangesAsync();

        var user1Clients = await _clientRepository.GetByUserIdAsync(user1.Id);

        user1Clients.Count.ShouldBe(2);
        user1Clients.ShouldAllBe(c => c.UserId == user1.Id);
    }

    [Fact]
    public async Task GetByIdAndUserIdAsync_ShouldReturnClient_WhenBelongsToUser()
    {
        var user = await CreateAndSaveUserAsync();
        var client = MakeClient(user.Id);

        await _clientRepository.AddAsync(client);
        await _unitOfWork.SaveChangesAsync();

        var found = await _clientRepository.GetByIdAndUserIdAsync(client.Id, user.Id);

        found.ShouldNotBeNull();
        found!.Id.ShouldBe(client.Id);
    }

    [Fact]
    public async Task GetByIdAndUserIdAsync_ShouldReturnNull_WhenClientBelongsToAnotherUser()
    {
        var user1 = await CreateAndSaveUserAsync();

        var email2 = Email.Create("outro@example.com").Value;
        var doc2 = Document.Create("11222333000181").Value;
        var user2 = User.Register("Outro User", email2, doc2, "$argon2id$hash2");
        await _userRepository.AddAsync(user2);
        await _unitOfWork.SaveChangesAsync();

        var client = MakeClient(user1.Id);
        await _clientRepository.AddAsync(client);
        await _unitOfWork.SaveChangesAsync();

        var found = await _clientRepository.GetByIdAndUserIdAsync(client.Id, user2.Id);

        found.ShouldBeNull();
    }
}
