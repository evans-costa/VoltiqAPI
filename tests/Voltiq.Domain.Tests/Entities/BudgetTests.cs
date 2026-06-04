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
        var item = BudgetItem.Create(budget.Id, null, BudgetItemType.MaoDeObra, null, 2, 15.50m, "Cabo 10mm");

        budget.AddItem(item);

        budget.TotalAmount.ShouldBe(31.00m);
        budget.Items.Count.ShouldBe(1);
    }

    [Fact]
    public void AddMultipleItems_ShouldSumAllTotals()
    {
        var budget = Budget.Register(ValidUserId, ValidClientId);
        var item1 = BudgetItem.Create(budget.Id, null, BudgetItemType.MaoDeObra, null, 2, 10m, "Cabo 10mm");
        var item2 = BudgetItem.Create(budget.Id, null, BudgetItemType.MaoDeObra, null, 1, 50m, "Disjuntor");

        budget.AddItem(item1);
        budget.AddItem(item2);

        budget.TotalAmount.ShouldBe(70m);
    }

    [Fact]
    public void Edit_WithValidData_ShouldUpdateBudgetAndRecalculateTotals()
    {
        var budget = Budget.Register(ValidUserId, ValidClientId);
        var oldItem = BudgetItem.Create(budget.Id, null, BudgetItemType.MaoDeObra, null, 2, 10m, "Cabo 10mm");
        budget.AddItem(oldItem);

        var newClientId = Guid.NewGuid();
        var newItem = BudgetItem.Create(budget.Id, null, BudgetItemType.MaoDeObra, null, 1, 50m, "Disjuntor");

        budget.Edit(newClientId, new[] { newItem });

        budget.ClientId.ShouldBe(newClientId);
        budget.TotalAmount.ShouldBe(50m);
        budget.Items.Count.ShouldBe(1);
        budget.Items.ShouldContain(newItem);
        budget.Items.ShouldNotContain(oldItem);
    }

    [Fact]
    public void Edit_WhenNotDraft_ShouldThrowDomainException()
    {
        var budget = Budget.Register(ValidUserId, ValidClientId);
        var newItem = BudgetItem.Create(budget.Id, null, BudgetItemType.MaoDeObra, null, 1, 50m, "Disjuntor");

        var statusProp = typeof(Budget).GetProperty(nameof(Budget.Status));
        statusProp!.SetValue(budget, BudgetStatus.Approved);

        Should.Throw<DomainException>(() =>
            budget.Edit(ValidClientId, new[] { newItem }))
            .Message.ShouldBe(ResourceErrorMessages.ORCAMENTO_APENAS_RASCUNHO_PODE_SER_EDITADO);
    }

    [Fact]
    public void Edit_WithEmptyClientId_ShouldThrowDomainException()
    {
        var budget = Budget.Register(ValidUserId, ValidClientId);
        var newItem = BudgetItem.Create(budget.Id, null, BudgetItemType.MaoDeObra, null, 1, 50m, "Disjuntor");

        Should.Throw<DomainException>(() =>
            budget.Edit(Guid.Empty, new[] { newItem }))
            .Message.ShouldBe(ResourceErrorMessages.ORCAMENTO_CLIENTE_OBRIGATORIO);
    }

    [Fact]
    public void Edit_WithEmptyItems_ShouldThrowDomainException()
    {
        var budget = Budget.Register(ValidUserId, ValidClientId);

        Should.Throw<DomainException>(() =>
            budget.Edit(ValidClientId, Array.Empty<BudgetItem>()))
            .Message.ShouldBe(ResourceErrorMessages.ORCAMENTO_ITEMS_OBRIGATORIOS);
    }

    [Fact]
    public void FinalizeBudget_WithValidDraftBudget_ShouldTransitionToFinalized()
    {
        var budget = Budget.Register(ValidUserId, ValidClientId);
        var item = BudgetItem.Create(budget.Id, null, BudgetItemType.MaoDeObra, null, 2, 10m, "Cabo");
        budget.AddItem(item);

        budget.FinalizeBudget();

        budget.Status.ShouldBe(BudgetStatus.Finalized);
        budget.DomainEvents.ShouldContain(e => e is BudgetFinalizedEvent);
    }

    [Fact]
    public void FinalizeBudget_WhenAlreadyFinalized_ShouldThrowDomainException()
    {
        var budget = Budget.Register(ValidUserId, ValidClientId);
        var item = BudgetItem.Create(budget.Id, null, BudgetItemType.MaoDeObra, null, 2, 10m, "Cabo");
        budget.AddItem(item);
        budget.FinalizeBudget();

        Should.Throw<DomainException>(() =>
            budget.FinalizeBudget())
            .Message.ShouldBe(ResourceErrorMessages.ORCAMENTO_APENAS_RASCUNHO_PODE_SER_FINALIZADO);
    }

    [Fact]
    public void FinalizeBudget_WithEmptyItems_ShouldThrowDomainException()
    {
        var budget = Budget.Register(ValidUserId, ValidClientId);

        Should.Throw<DomainException>(() =>
            budget.FinalizeBudget())
            .Message.ShouldBe(ResourceErrorMessages.ORCAMENTO_ITEMS_OBRIGATORIOS);
    }

    [Fact]
    public void Approve_WithFinalizedBudget_ShouldTransitionToApproved()
    {
        var budget = Budget.Register(ValidUserId, ValidClientId);
        var item = BudgetItem.Create(budget.Id, null, BudgetItemType.MaoDeObra, null, 2, 10m, "Cabo");
        budget.AddItem(item);
        budget.FinalizeBudget();

        budget.Approve();

        budget.Status.ShouldBe(BudgetStatus.Approved);
        budget.DomainEvents.ShouldContain(e => e is BudgetApprovedEvent);
    }

    [Fact]
    public void Approve_WhenDraftBudget_ShouldThrowDomainException()
    {
        var budget = Budget.Register(ValidUserId, ValidClientId);

        Should.Throw<DomainException>(() =>
            budget.Approve())
            .Message.ShouldBe(ResourceErrorMessages.ORCAMENTO_STATUS_INVALIDO_PARA_APROVACAO);
    }

    [Fact]
    public void Reject_WithFinalizedBudget_ShouldTransitionToRejected()
    {
        var budget = Budget.Register(ValidUserId, ValidClientId);
        var item = BudgetItem.Create(budget.Id, null, BudgetItemType.MaoDeObra, null, 2, 10m, "Cabo");
        budget.AddItem(item);
        budget.FinalizeBudget();

        budget.Reject();

        budget.Status.ShouldBe(BudgetStatus.Rejected);
        budget.DomainEvents.ShouldContain(e => e is BudgetRejectedEvent);
    }

    [Fact]
    public void Reject_WhenDraftBudget_ShouldThrowDomainException()
    {
        var budget = Budget.Register(ValidUserId, ValidClientId);

        Should.Throw<DomainException>(() =>
            budget.Reject())
            .Message.ShouldBe(ResourceErrorMessages.ORCAMENTO_STATUS_INVALIDO_PARA_REJEICAO);
    }
}
