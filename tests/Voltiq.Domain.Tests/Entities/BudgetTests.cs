using Shouldly;
using Voltiq.Domain.Entities;
using Voltiq.Domain.Enums;
using Voltiq.Domain.Events;
using Voltiq.Exceptions.Exceptions;
using Voltiq.Exceptions.Resources;

namespace Voltiq.Domain.Tests.Entities;

public class BudgetTests
{
    private static readonly Guid ValidUserId = Guid.NewGuid();
    private static readonly Guid ValidClientId = Guid.NewGuid();

    [Fact]
    public void Register_WithValidData_ShouldRegisterBudget()
    {
        var budget = Budget.Register(ValidUserId, ValidClientId);

        budget.Id.ShouldNotBe(Guid.Empty);
        budget.UserId.ShouldBe(ValidUserId);
        budget.ClientId.ShouldBe(ValidClientId);
        budget.TotalAmount.ShouldBe(0m);
        budget.Status.ShouldBe(BudgetStatus.Draft);
    }

    [Fact]
    public void Register_ShouldRaise_BudgetRegisteredEvent()
    {
        var budget = Budget.Register(ValidUserId, ValidClientId);

        budget.DomainEvents.ShouldContain(e => e is BudgetRegisteredEvent);
    }

    [Fact]
    public void Register_WithEmptyClientId_ShouldThrowDomainException()
    {
        Should.Throw<DomainException>(() =>
            Budget.Register(ValidUserId, Guid.Empty))
            .Message.ShouldBe(ResourceErrorMessages.ORCAMENTO_CLIENTE_OBRIGATORIO);
    }

    [Fact]
    public void Register_WithEmptyUserId_ShouldThrowDomainException()
    {
        Should.Throw<DomainException>(() =>
            Budget.Register(Guid.Empty, ValidClientId))
            .Message.ShouldBe(ResourceErrorMessages.ORCAMENTO_USUARIO_OBRIGATORIO);
    }

    [Fact]
    public void AddItem_ShouldRecalculateTotalAmount()
    {
        var budget = Budget.Register(ValidUserId, ValidClientId);
        var item = BudgetItem.Create(budget.Id, null, "Cabo 10mm", MaterialUnit.Metro, 2, 15.50m);

        budget.AddItem(item);

        budget.TotalAmount.ShouldBe(31.00m);
        budget.Items.Count.ShouldBe(1);
    }

    [Fact]
    public void AddMultipleItems_ShouldSumAllTotals()
    {
        var budget = Budget.Register(ValidUserId, ValidClientId);
        var item1 = BudgetItem.Create(budget.Id, null, "Cabo 10mm", MaterialUnit.Metro, 2, 10m);
        var item2 = BudgetItem.Create(budget.Id, null, "Disjuntor", MaterialUnit.Unidade, 1, 50m);

        budget.AddItem(item1);
        budget.AddItem(item2);

        budget.TotalAmount.ShouldBe(70m);
    }
}
