using Microsoft.EntityFrameworkCore;
using Shouldly;
using Voltiq.CommonTestUtilities.Builders;
using Voltiq.CommonTestUtilities.Database;
using Voltiq.CommonTestUtilities.Fixtures;
using Voltiq.Domain.Entities;
using Voltiq.Domain.Enums;
using Voltiq.Domain.Interfaces.Repositories.Budget;
using Voltiq.Infrastructure.Persistence;
using Voltiq.Infrastructure.Persistence.Repositories;
using Voltiq.Infrastructure.Persistence.Repositories.Budget;
using Voltiq.Infrastructure.Persistence.Repositories.Client;
using Voltiq.Infrastructure.Persistence.Repositories.User;

namespace Voltiq.Infrastructure.Tests.Persistence;

public class BudgetRepositoryTests(PostgreSqlContainerFixture fixture)
    : IClassFixture<PostgreSqlContainerFixture>, IAsyncLifetime
{
    private static readonly Guid UserId = Guid.NewGuid();
    private IBudgetReadOnlyRepository _budgetReadOnly = null!;

    private BudgetRepository _budgetRepository = null!;
    private ClientRepository _clientRepository = null!;
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
        _budgetRepository = new BudgetRepository(_dbContext);
        _budgetReadOnly = _budgetRepository;
        _unitOfWork = new UnitOfWork(_dbContext);
    }

    public async ValueTask DisposeAsync()
    {
        await _dbContext.DisposeAsync();
    }

    [Fact]
    public async Task AddAndGetById_ShouldPersistBudget()
    {
        var user = await TestDataBuilder.SeedUserAsync(_userRepository, _unitOfWork);
        var client = await TestDataBuilder.SeedClientAsync(_clientRepository, _unitOfWork, user.Id);
        var budget =
            await TestDataBuilder.SeedBudgetAsync(_budgetRepository, _unitOfWork, user.Id,
                client.Id);

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
        var user1 = await TestDataBuilder.SeedUserAsync(_userRepository, _unitOfWork);
        var user2 = await TestDataBuilder.SeedUserAsync(_userRepository, _unitOfWork,
            email: "maria@example.com", document: "11222333000181");

        var client1 =
            await TestDataBuilder.SeedClientAsync(_clientRepository, _unitOfWork, user1.Id);
        var client2 = await TestDataBuilder.SeedClientAsync(_clientRepository, _unitOfWork,
            user2.Id,
            email: "cliente2@example.com");

        await TestDataBuilder.SeedBudgetAsync(_budgetRepository, _unitOfWork, user1.Id, client1.Id);
        await TestDataBuilder.SeedBudgetAsync(_budgetRepository, _unitOfWork, user1.Id, client1.Id);
        await TestDataBuilder.SeedBudgetAsync(_budgetRepository, _unitOfWork, user2.Id, client2.Id);

        var user1Budgets =
            await _budgetRepository.GetByUserIdAsync(user1.Id,
                TestContext.Current.CancellationToken);

        user1Budgets.Count.ShouldBe(2);
        user1Budgets.ShouldAllBe(b => b.UserId == user1.Id);
    }

    [Fact]
    public async Task GetByIdAndUserIdAsync_ShouldReturnBudget_WhenBelongsToUser()
    {
        var user = await TestDataBuilder.SeedUserAsync(_userRepository, _unitOfWork);
        var client = await TestDataBuilder.SeedClientAsync(_clientRepository, _unitOfWork, user.Id);
        var budget =
            await TestDataBuilder.SeedBudgetAsync(_budgetRepository, _unitOfWork, user.Id,
                client.Id);

        var found = await _budgetReadOnly.GetByIdAndUserIdAsync(budget.Id, user.Id,
            TestContext.Current.CancellationToken);

        found.ShouldNotBeNull();
        found.Id.ShouldBe(budget.Id);
    }

    [Fact]
    public async Task GetByIdAndUserIdAsync_ShouldReturnNull_WhenBudgetBelongsToAnotherUser()
    {
        var user1 = await TestDataBuilder.SeedUserAsync(_userRepository, _unitOfWork);
        var user2 = await TestDataBuilder.SeedUserAsync(_userRepository, _unitOfWork,
            email: "outro@example.com", document: "11222333000181");

        var client1 =
            await TestDataBuilder.SeedClientAsync(_clientRepository, _unitOfWork, user1.Id);
        var budget =
            await TestDataBuilder.SeedBudgetAsync(_budgetRepository, _unitOfWork, user1.Id,
                client1.Id);

        var found = await _budgetReadOnly.GetByIdAndUserIdAsync(budget.Id, user2.Id,
            TestContext.Current.CancellationToken);

        found.ShouldBeNull();
    }

    [Fact]
    public async Task GetByIdWithItemsAndUserIdAsync_ShouldReturnBudgetWithItems()
    {
        var user = await TestDataBuilder.SeedUserAsync(_userRepository, _unitOfWork);
        var client = await TestDataBuilder.SeedClientAsync(_clientRepository, _unitOfWork, user.Id);

        var budget = TestDataBuilder.MakeBudget(user.Id, client.Id);
        var item = BudgetItem.Create(budget.Id, null, BudgetItemType.MaoDeObra, null, 2, 15.50m, "Cabo 10mm");
        budget.AddItem(item);
        await _budgetRepository.AddAsync(budget, TestContext.Current.CancellationToken);
        await _unitOfWork.SaveChangesAsync(TestContext.Current.CancellationToken);

        var found = await _budgetReadOnly.GetByIdWithItemsAndUserIdAsync(budget.Id, user.Id,
            TestContext.Current.CancellationToken);

        found.ShouldNotBeNull();
        found.Items.Count.ShouldBe(1);
        found.TotalAmount.ShouldBe(31.00m);
    }

    [Fact]
    public async Task GetByIdWithItemsAndUserIdAsync_ShouldReturnNull_WhenBelongsToAnotherUser()
    {
        var user1 = await TestDataBuilder.SeedUserAsync(_userRepository, _unitOfWork);
        var user2 = await TestDataBuilder.SeedUserAsync(_userRepository, _unitOfWork,
            email: "outro@example.com", document: "11222333000181");
        var client1 =
            await TestDataBuilder.SeedClientAsync(_clientRepository, _unitOfWork, user1.Id);
        var budget =
            await TestDataBuilder.SeedBudgetAsync(_budgetRepository, _unitOfWork, user1.Id,
                client1.Id);

        var found = await _budgetReadOnly.GetByIdWithItemsAndUserIdAsync(budget.Id, user2.Id,
            TestContext.Current.CancellationToken);

        found.ShouldBeNull();
    }

    [Fact]
    public async Task GetByClientIdAsync_ShouldReturnOnlyBudgetsOfClient()
    {
        var user = await TestDataBuilder.SeedUserAsync(_userRepository, _unitOfWork);
        var client1 = await TestDataBuilder.SeedClientAsync(_clientRepository, _unitOfWork, user.Id);
        var client2 = await TestDataBuilder.SeedClientAsync(_clientRepository, _unitOfWork, user.Id, email: "outro@example.com");

        await TestDataBuilder.SeedBudgetAsync(_budgetRepository, _unitOfWork, user.Id, client1.Id);
        await TestDataBuilder.SeedBudgetAsync(_budgetRepository, _unitOfWork, user.Id, client2.Id);

        var budgets = await _budgetRepository.GetByClientIdAsync(client1.Id, TestContext.Current.CancellationToken);

        budgets.Count.ShouldBe(1);
        budgets.ShouldAllBe(b => b.ClientId == client1.Id);
    }

    [Fact]
    public async Task GetByIdWithItemsAsync_ShouldReturnBudgetWithItems_WhenBudgetExists()
    {
        var user = await TestDataBuilder.SeedUserAsync(_userRepository, _unitOfWork);
        var client = await TestDataBuilder.SeedClientAsync(_clientRepository, _unitOfWork, user.Id);
        var budget = TestDataBuilder.MakeBudget(user.Id, client.Id);
        var item = BudgetItem.Create(budget.Id, null, BudgetItemType.MaoDeObra, null, 2, 10.0m, "Instalação");
        budget.AddItem(item);
        await _budgetRepository.AddAsync(budget, TestContext.Current.CancellationToken);
        await _unitOfWork.SaveChangesAsync(TestContext.Current.CancellationToken);

        var found = await _budgetRepository.GetByIdWithItemsAsync(budget.Id, TestContext.Current.CancellationToken);

        found.ShouldNotBeNull();
        found.Items.Count.ShouldBe(1);
        found.Items.First().MaterialName.ShouldBe("Instalação");
    }

    [Fact]
    public async Task Remove_ShouldDeleteBudget()
    {
        var user = await TestDataBuilder.SeedUserAsync(_userRepository, _unitOfWork);
        var client = await TestDataBuilder.SeedClientAsync(_clientRepository, _unitOfWork, user.Id);
        var budget = await TestDataBuilder.SeedBudgetAsync(_budgetRepository, _unitOfWork, user.Id, client.Id);

        _budgetRepository.Remove(budget);
        await _unitOfWork.SaveChangesAsync(TestContext.Current.CancellationToken);

        var found = await _budgetRepository.GetByIdAsync(budget.Id, TestContext.Current.CancellationToken);
        found.ShouldBeNull();
    }

    [Fact]
    public async Task GetTrackedByIdAndUserIdAsync_ShouldReturnTrackedBudget_WhenBelongsToUser()
    {
        var user = await TestDataBuilder.SeedUserAsync(_userRepository, _unitOfWork);
        var client = await TestDataBuilder.SeedClientAsync(_clientRepository, _unitOfWork, user.Id);
        var budget = await TestDataBuilder.SeedBudgetAsync(_budgetRepository, _unitOfWork, user.Id, client.Id);

        _dbContext.ChangeTracker.Clear();

        var found = await _budgetRepository.GetTrackedByIdAndUserIdAsync(budget.Id, user.Id, TestContext.Current.CancellationToken);

        found.ShouldNotBeNull();
        _dbContext.Entry(found).State.ShouldBe(EntityState.Unchanged);
    }

    [Fact]
    public async Task GetTrackedByIdWithItemsAndUserIdAsync_ShouldReturnTrackedBudgetWithItems_WhenBelongsToUser()
    {
        var user = await TestDataBuilder.SeedUserAsync(_userRepository, _unitOfWork);
        var client = await TestDataBuilder.SeedClientAsync(_clientRepository, _unitOfWork, user.Id);
        var budget = TestDataBuilder.MakeBudget(user.Id, client.Id);
        var item = BudgetItem.Create(budget.Id, null, BudgetItemType.MaoDeObra, null, 2, 10.0m, "Instalação");
        budget.AddItem(item);
        await _budgetRepository.AddAsync(budget, TestContext.Current.CancellationToken);
        await _unitOfWork.SaveChangesAsync(TestContext.Current.CancellationToken);

        _dbContext.ChangeTracker.Clear();

        var found = await _budgetRepository.GetTrackedByIdWithItemsAndUserIdAsync(budget.Id, user.Id, TestContext.Current.CancellationToken);

        found.ShouldNotBeNull();
        found.Items.Count.ShouldBe(1);
        _dbContext.Entry(found).State.ShouldBe(EntityState.Unchanged);
    }

    [Fact]
    public async Task GetTrackedByIdAsync_ShouldReturnTrackedBudget_WhenExists()
    {
        var user = await TestDataBuilder.SeedUserAsync(_userRepository, _unitOfWork);
        var client = await TestDataBuilder.SeedClientAsync(_clientRepository, _unitOfWork, user.Id);
        var budget = await TestDataBuilder.SeedBudgetAsync(_budgetRepository, _unitOfWork, user.Id, client.Id);

        _dbContext.ChangeTracker.Clear();

        var found = await _budgetRepository.GetTrackedByIdAsync(budget.Id, TestContext.Current.CancellationToken);

        found.ShouldNotBeNull();
        _dbContext.Entry(found).State.ShouldBe(EntityState.Unchanged);
    }

    [Fact]
    public async Task GetByUserIdWithClientAsync_ShouldReturnBudgetsWithClientLoaded()
    {
        var user = await TestDataBuilder.SeedUserAsync(_userRepository, _unitOfWork);
        var client = await TestDataBuilder.SeedClientAsync(_clientRepository, _unitOfWork, user.Id);
        await TestDataBuilder.SeedBudgetAsync(_budgetRepository, _unitOfWork, user.Id, client.Id);

        var budgets = await _budgetRepository.GetByUserIdWithClientAsync(user.Id, TestContext.Current.CancellationToken);

        budgets.Count.ShouldBe(1);
        budgets.First().Client.ShouldNotBeNull();
        budgets.First().Client.Name.ShouldBe(client.Name);
    }

    [Fact]
    public async Task GetByIdWithItemsAndClientAsync_ShouldReturnBudgetWithItemsAndClientLoaded()
    {
        var user = await TestDataBuilder.SeedUserAsync(_userRepository, _unitOfWork);
        var client = await TestDataBuilder.SeedClientAsync(_clientRepository, _unitOfWork, user.Id);
        var budget = TestDataBuilder.MakeBudget(user.Id, client.Id);
        var item = BudgetItem.Create(budget.Id, null, BudgetItemType.MaoDeObra, null, 2, 10.0m, "Instalação");
        budget.AddItem(item);
        await _budgetRepository.AddAsync(budget, TestContext.Current.CancellationToken);
        await _unitOfWork.SaveChangesAsync(TestContext.Current.CancellationToken);

        var found = await _budgetRepository.GetByIdWithItemsAndClientAsync(budget.Id, user.Id, TestContext.Current.CancellationToken);

        found.ShouldNotBeNull();
        found.Items.Count.ShouldBe(1);
        found.Client.ShouldNotBeNull();
        found.Client.Name.ShouldBe(client.Name);
    }
}
