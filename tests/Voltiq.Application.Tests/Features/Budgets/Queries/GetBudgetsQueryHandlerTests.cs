using ErrorOr;
using Moq;
using Shouldly;
using Voltiq.Application.Features.Budgets;
using Voltiq.Application.Features.Budgets.Queries.GetBudgets;
using Voltiq.Domain.Entities;
using Voltiq.Domain.Enums;
using Voltiq.Domain.Interfaces.Repositories.Budget;
using Voltiq.Domain.ValueObjects;

namespace Voltiq.Application.Tests.Features.Budgets.Queries;

public class GetBudgetsQueryHandlerTests
{
    private readonly Mock<IBudgetReadOnlyRepository> _budgetReadRepoMock = new();

    private readonly Guid _userId = Guid.NewGuid();

    private GetBudgetsQueryHandler CreateHandler() =>
        new(_budgetReadRepoMock.Object);

    private static Budget MakeBudgetWithClient(Guid userId)
    {
        var client = Client.Register(userId, "João Silva", "(11) 99999-9999",
            Email.Create("joao@example.com").Value,
            Address.Create("Rua das Flores", "123", "São Paulo", "SP", "01310-100"));

        var budget = Budget.Register(userId, client.Id);
        budget.AddItem(BudgetItem.Create(budget.Id, null, BudgetItemType.MaoDeObra, null, 2, 15.50m, "Cabo 10mm"));

        typeof(Budget).GetProperty("Client")!.SetValue(budget, client);

        return budget;
    }

    [Fact]
    public async Task Handle_ShouldReturnBudgetSummariesForUser()
    {
        var budgets = new List<Budget> { MakeBudgetWithClient(_userId) };
        _budgetReadRepoMock
            .Setup(r => r.GetByUserIdWithClientAsync(_userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(budgets);

        var handler = CreateHandler();
        var result = await handler.Handle(new GetBudgetsQuery { UserId = _userId }, CancellationToken.None);

        result.IsError.ShouldBeFalse();
        result.Value.Count.ShouldBe(1);

        var summary = result.Value[0];
        summary.Id.ShouldBe(budgets[0].Id);
        summary.Status.ShouldBe(BudgetStatus.Draft);
        summary.TotalAmount.ShouldBe(31.00m);
        summary.Client.Name.ShouldBe("João Silva");
    }

    [Fact]
    public async Task Handle_WhenNoBudgets_ShouldReturnEmptyList()
    {
        _budgetReadRepoMock
            .Setup(r => r.GetByUserIdWithClientAsync(_userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);

        var handler = CreateHandler();
        var result = await handler.Handle(new GetBudgetsQuery { UserId = _userId }, CancellationToken.None);

        result.IsError.ShouldBeFalse();
        result.Value.ShouldBeEmpty();
    }
}
