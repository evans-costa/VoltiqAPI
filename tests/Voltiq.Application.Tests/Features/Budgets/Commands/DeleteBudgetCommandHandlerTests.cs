using ErrorOr;
using Moq;
using Shouldly;
using Voltiq.Application.Features.Budgets.Commands.DeleteBudget;
using Voltiq.Domain.Entities;
using Voltiq.Domain.Interfaces;
using Voltiq.Domain.Interfaces.Repositories.Budget;
using Voltiq.Exceptions.Resources;

namespace Voltiq.Application.Tests.Features.Budgets.Commands;

public class DeleteBudgetCommandHandlerTests
{
    private readonly Mock<IBudgetUpdateOnlyRepository> _budgetUpdateRepoMock = new();
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();

    private readonly Guid _userId = Guid.NewGuid();
    private readonly Guid _budgetId = Guid.NewGuid();

    private DeleteBudgetCommandHandler CreateHandler() =>
        new(_budgetUpdateRepoMock.Object, _unitOfWorkMock.Object);

    [Fact]
    public async Task Handle_WhenBudgetExists_ShouldDeleteAndReturnDeleted()
    {
        var budget = Budget.Register(_userId, Guid.NewGuid());
        _budgetUpdateRepoMock
            .Setup(r => r.GetTrackedByIdAndUserIdAsync(_budgetId, _userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(budget);

        var handler = CreateHandler();
        var result = await handler.Handle(new DeleteBudgetCommand(_budgetId) { UserId = _userId }, CancellationToken.None);

        result.IsError.ShouldBeFalse();
        _budgetUpdateRepoMock.Verify(r => r.Remove(budget), Times.Once);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenBudgetNotFound_ShouldReturnNotFoundError()
    {
        _budgetUpdateRepoMock
            .Setup(r => r.GetTrackedByIdAndUserIdAsync(_budgetId, _userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Budget?)null);

        var handler = CreateHandler();
        var result = await handler.Handle(new DeleteBudgetCommand(_budgetId) { UserId = _userId }, CancellationToken.None);

        result.IsError.ShouldBeTrue();
        result.FirstError.Type.ShouldBe(ErrorType.NotFound);
        result.FirstError.Description.ShouldBe(ResourceErrorMessages.ORCAMENTO_NAO_ENCONTRADO);
        _budgetUpdateRepoMock.Verify(r => r.Remove(It.IsAny<Budget>()), Times.Never);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
}
