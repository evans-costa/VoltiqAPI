using Microsoft.EntityFrameworkCore;
using Shouldly;
using Voltiq.CommonTestUtilities.Database;
using Voltiq.CommonTestUtilities.Fixtures;
using Voltiq.Domain.Entities;
using Voltiq.Domain.Enums;
using Voltiq.Domain.ValueObjects;
using Voltiq.Infrastructure.Persistence;
using Voltiq.Infrastructure.Persistence.Repositories;
using Voltiq.Infrastructure.Persistence.Repositories.Budget;
using Voltiq.Infrastructure.Persistence.Repositories.Client;

namespace Voltiq.Infrastructure.Tests.Persistence;

public class BudgetRepositoryTests(PostgreSqlContainerFixture fixture)
    : IClassFixture<PostgreSqlContainerFixture>, IAsyncLifetime
{
    private static readonly Guid UserId = Guid.NewGuid();

    private BudgetRepository _budgetRepository = null!;
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
        _budgetRepository = new BudgetRepository(_dbContext);
        _unitOfWork = new UnitOfWork(_dbContext);
    }

    public async ValueTask DisposeAsync()
    {
        await _dbContext.DisposeAsync();
    }

    private static User MakeUser(string email = "joao@example.com", string doc = "529.982.247-25")
    {
        var userEmail = Email.Create(email).Value;
        var userDocument = Document.Create(doc).Value;
        return User.Register("João Silva", userEmail, userDocument, "$argon2id$hash");
    }

    private static Client MakeClient(Guid userId, string email = "cliente@example.com")
    {
        var clientEmail = Email.Create(email).Value;
        return Client.Register(userId, "Cliente Teste", "(11) 99999-9999", clientEmail,
            Address.Create("Rua das Flores", "123", "São Paulo", "SP", "01310-100"));
    }

    private static Budget MakeBudget(Guid userId, Guid clientId)
    {
        return Budget.Register(userId, clientId);
    }

    private async Task<User> CreateAndSaveUserAsync(string email = "joao@example.com",
        string doc = "529.982.247-25")
    {
        var user = MakeUser(email, doc);
        await _userRepository.AddAsync(user);
        await _unitOfWork.SaveChangesAsync();
        return user;
    }

    private async Task<Client> CreateAndSaveClientAsync(Guid userId)
    {
        var client = MakeClient(userId);
        await _clientRepository.AddAsync(client);
        await _unitOfWork.SaveChangesAsync();
        return client;
    }

    [Fact]
    public async Task AddAndGetById_ShouldPersistBudget()
    {
        var user = await CreateAndSaveUserAsync();
        var client = await CreateAndSaveClientAsync(user.Id);
        var budget = MakeBudget(user.Id, client.Id);

        await _budgetRepository.AddAsync(budget, TestContext.Current.CancellationToken);
        await _unitOfWork.SaveChangesAsync(TestContext.Current.CancellationToken);

        var found =
            await _budgetRepository.GetByIdAsync(budget.Id, TestContext.Current.CancellationToken);

        found.ShouldNotBeNull();
        found.Id.ShouldBe(budget.Id);
        found.UserId.ShouldBe(user.Id);
        found.ClientId.ShouldBe(client.Id);
        found.Status.ShouldBe(BudgetStatus.Draft);
        found.TotalAmount.ShouldBe(0m);
    }

    [Fact]
    public async Task GetByUserIdAsync_ShouldReturnOnlyBudgetsOfUser()
    {
        var user1 = await CreateAndSaveUserAsync();
        var user2 = await CreateAndSaveUserAsync("maria@example.com", "11222333000181");

        var client1 = await CreateAndSaveClientAsync(user1.Id);
        var client2 = await CreateAndSaveClientAsync(user2.Id);

        await _budgetRepository.AddAsync(MakeBudget(user1.Id, client1.Id),
            TestContext.Current.CancellationToken);
        await _budgetRepository.AddAsync(MakeBudget(user1.Id, client1.Id),
            TestContext.Current.CancellationToken);
        await _budgetRepository.AddAsync(MakeBudget(user2.Id, client2.Id),
            TestContext.Current.CancellationToken);
        await _unitOfWork.SaveChangesAsync(TestContext.Current.CancellationToken);

        var user1Budgets =
            await _budgetRepository.GetByUserIdAsync(user1.Id,
                TestContext.Current.CancellationToken);

        user1Budgets.Count.ShouldBe(2);
        user1Budgets.ShouldAllBe(b => b.UserId == user1.Id);
    }

    [Fact]
    public async Task GetByIdAndUserIdAsync_ShouldReturnBudget_WhenBelongsToUser()
    {
        var user = await CreateAndSaveUserAsync();
        var client = await CreateAndSaveClientAsync(user.Id);
        var budget = MakeBudget(user.Id, client.Id);

        await _budgetRepository.AddAsync(budget, TestContext.Current.CancellationToken);
        await _unitOfWork.SaveChangesAsync(TestContext.Current.CancellationToken);

        var found = await _budgetRepository.GetByIdAndUserIdAsync(budget.Id, user.Id,
            TestContext.Current.CancellationToken);

        found.ShouldNotBeNull();
        found.Id.ShouldBe(budget.Id);
    }

    [Fact]
    public async Task GetByIdAndUserIdAsync_ShouldReturnNull_WhenBudgetBelongsToAnotherUser()
    {
        var user1 = await CreateAndSaveUserAsync();
        var user2 = await CreateAndSaveUserAsync("outro@example.com", "11222333000181");

        var client1 = await CreateAndSaveClientAsync(user1.Id);
        var budget = MakeBudget(user1.Id, client1.Id);

        await _budgetRepository.AddAsync(budget, TestContext.Current.CancellationToken);
        await _unitOfWork.SaveChangesAsync(TestContext.Current.CancellationToken);

        var found = await _budgetRepository.GetByIdAndUserIdAsync(budget.Id, user2.Id,
            TestContext.Current.CancellationToken);

        found.ShouldBeNull();
    }

    [Fact]
    public async Task GetByIdWithItemsAndUserIdAsync_ShouldReturnBudgetWithItems()
    {
        var user = await CreateAndSaveUserAsync();
        var client = await CreateAndSaveClientAsync(user.Id);
        var budget = MakeBudget(user.Id, client.Id);
        var item = BudgetItem.Create(budget.Id, null, "Cabo 10mm", MaterialUnit.Metro, 2, 15.50m);
        budget.AddItem(item);

        await _budgetRepository.AddAsync(budget, TestContext.Current.CancellationToken);
        await _unitOfWork.SaveChangesAsync(TestContext.Current.CancellationToken);

        var found = await _budgetRepository.GetByIdWithItemsAndUserIdAsync(budget.Id, user.Id,
            TestContext.Current.CancellationToken);

        found.ShouldNotBeNull();
        found.Items.Count.ShouldBe(1);
        found.TotalAmount.ShouldBe(31.00m);
    }

    [Fact]
    public async Task GetByIdWithItemsAndUserIdAsync_ShouldReturnNull_WhenBelongsToAnotherUser()
    {
        var user1 = await CreateAndSaveUserAsync();
        var user2 = await CreateAndSaveUserAsync("outro@example.com", "11222333000181");
        var client1 = await CreateAndSaveClientAsync(user1.Id);
        var budget = MakeBudget(user1.Id, client1.Id);

        await _budgetRepository.AddAsync(budget, TestContext.Current.CancellationToken);
        await _unitOfWork.SaveChangesAsync(TestContext.Current.CancellationToken);

        var found = await _budgetRepository.GetByIdWithItemsAndUserIdAsync(budget.Id, user2.Id,
            TestContext.Current.CancellationToken);

        found.ShouldBeNull();
    }
}
