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
        var item = BudgetItem.Create(budget.Id, null, "Cabo 10mm", MaterialUnit.Metro, 2, 15.50m);
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
}
