using ErrorOr;
using Moq;
using Shouldly;
using Voltiq.Application.Features.Budgets.Queries.GetBudgetById;
using Voltiq.Domain.Entities;
using Voltiq.Domain.Enums;
using Voltiq.Domain.Interfaces.Repositories.Budget;
using Voltiq.Domain.ValueObjects;
using Voltiq.Exceptions.Resources;

namespace Voltiq.Application.Tests.Features.Budgets.Queries;

public class GetBudgetByIdQueryHandlerTests
{
    private readonly Guid _budgetId = Guid.NewGuid();
    private readonly Mock<IBudgetReadOnlyRepository> _budgetReadRepoMock = new();

    private readonly Guid _userId = Guid.NewGuid();

    private GetBudgetByIdQueryHandler CreateHandler()
    {
        return new GetBudgetByIdQueryHandler(_budgetReadRepoMock.Object);
    }

    private static Budget MakeBudgetWithItemsAndClient(Guid userId, Guid budgetId)
    {
        var client = Client.Register(userId, "Maria Souza", "(21) 98888-7777",
            Email.Create("maria@example.com").Value,
            Address.Create("Av. Brasil", "500", "Rio de Janeiro", "RJ", "20040-020"));

        var budget = Budget.Register(userId, client.Id);
        typeof(Budget).GetProperty("Id")!.SetValue(budget, budgetId);
        budget.AddItem(BudgetItem.Create(budgetId, null, BudgetItemType.MaoDeObra, null, 5, 8.00m,
            "Fio elétrico"));
        typeof(Budget).GetProperty("Client")!.SetValue(budget, client);

        return budget;
    }

    [Fact]
    public async Task Handle_WhenBudgetExists_ShouldReturnDetailResponse()
    {
        var budget = MakeBudgetWithItemsAndClient(_userId, _budgetId);
        _budgetReadRepoMock
            .Setup(r =>
                r.GetByIdWithItemsAndClientAsync(_budgetId, _userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(budget);

        var handler = CreateHandler();
        var result = await handler.Handle(
            new GetBudgetByIdQuery(_budgetId) { UserId = _userId },
            CancellationToken.None);

        result.IsError.ShouldBeFalse();
        result.Value.Id.ShouldBe(_budgetId);
        result.Value.Status.ShouldBe(BudgetStatus.Draft);
        result.Value.TotalAmount.ShouldBe(40.00m);
        result.Value.Client.Name.ShouldBe("Maria Souza");
        result.Value.Client.Phone.ShouldBe("(21) 98888-7777");
        result.Value.Client.Email.ShouldBe("maria@example.com");
        result.Value.Items.Count.ShouldBe(1);
        result.Value.Items[0].MaterialName.ShouldBe("Fio elétrico");
        result.Value.Items[0].TotalPrice.ShouldBe(40.00m);
    }

    [Fact]
    public async Task Handle_WhenBudgetNotFound_ShouldReturnNotFoundError()
    {
        _budgetReadRepoMock
            .Setup(r =>
                r.GetByIdWithItemsAndClientAsync(_budgetId, _userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Budget?)null);

        var handler = CreateHandler();
        var result = await handler.Handle(
            new GetBudgetByIdQuery(_budgetId) { UserId = _userId },
            CancellationToken.None);

        result.IsError.ShouldBeTrue();
        result.FirstError.Type.ShouldBe(ErrorType.NotFound);
        result.FirstError.Description.ShouldBe(ResourceErrorMessages.ORCAMENTO_NAO_ENCONTRADO);
    }
}
