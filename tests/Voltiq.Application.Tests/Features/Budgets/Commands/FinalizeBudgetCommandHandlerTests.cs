using ErrorOr;
using Moq;
using Shouldly;
using Voltiq.Application.Features.Budgets.Commands.FinalizeBudget;
using Voltiq.Domain.Entities;
using Voltiq.Domain.Enums;
using Voltiq.Domain.Interfaces;
using Voltiq.Domain.Interfaces.Repositories.Budget;
using Voltiq.Exceptions.Resources;

namespace Voltiq.Application.Tests.Features.Budgets.Commands;

public class FinalizeBudgetCommandHandlerTests
{
    private readonly Mock<IBudgetUpdateOnlyRepository> _budgetUpdateRepoMock = new();
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
    
    private readonly Guid _userId = Guid.NewGuid();
    private readonly Guid _budgetId = Guid.NewGuid();
    private readonly Guid _clientId = Guid.NewGuid();

    private FinalizeBudgetCommandHandler CreateHandler()
    {
        return new FinalizeBudgetCommandHandler(
            _budgetUpdateRepoMock.Object,
            _unitOfWorkMock.Object);
    }

    [Fact]
    public async Task Handle_WithValidDraftBudget_ShouldFinalizeAndSave()
    {
        // Arrange
        var budget = Budget.Register(_userId, _clientId);
        var item = BudgetItem.Create(budget.Id, null, BudgetItemType.MaoDeObra, null, 2, 15.50m, "Cabo 10mm");
        budget.AddItem(item);

        _budgetUpdateRepoMock
            .Setup(r => r.GetTrackedByIdWithItemsAndUserIdAsync(_budgetId, _userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(budget);

        var command = new FinalizeBudgetCommand(_budgetId) { UserId = _userId };
        var handler = CreateHandler();

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsError.ShouldBeFalse();
        result.Value.ShouldBe(Result.Updated);

        budget.Status.ShouldBe(BudgetStatus.Finalized);

        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenBudgetNotFound_ShouldReturnNotFound()
    {
        // Arrange
        _budgetUpdateRepoMock
            .Setup(r => r.GetTrackedByIdWithItemsAndUserIdAsync(_budgetId, _userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Budget?)null);

        var command = new FinalizeBudgetCommand(_budgetId) { UserId = _userId };
        var handler = CreateHandler();

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsError.ShouldBeTrue();
        result.FirstError.Type.ShouldBe(ErrorType.NotFound);
        result.FirstError.Description.ShouldBe(ResourceErrorMessages.ORCAMENTO_NAO_ENCONTRADO);

        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
}
