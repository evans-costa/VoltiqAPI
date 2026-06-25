using ErrorOr;
using Moq;
using Shouldly;
using Voltiq.Application.Common.Interfaces.Queue;
using Voltiq.Application.Features.Budgets.Commands.GenerateBudgetPdf;
using Voltiq.Domain.Entities;
using Voltiq.Domain.Enums;
using Voltiq.Domain.Interfaces.Repositories.Budget;
using Voltiq.Exceptions.Resources;

namespace Voltiq.Application.Tests.Features.Budgets.Commands;

public class GenerateBudgetPdfCommandHandlerTests
{
    private readonly Mock<IBudgetReadOnlyRepository> _budgetRepoMock = new();
    private readonly Mock<IQueueService> _queueServiceMock = new();
    private readonly Guid _budgetId = Guid.NewGuid();
    private readonly Guid _userId = Guid.NewGuid();

    private GenerateBudgetPdfCommandHandler CreateHandler()
    {
        return new GenerateBudgetPdfCommandHandler(_queueServiceMock.Object, _budgetRepoMock.Object);
    }

    [Fact]
    public async Task Handle_WhenBudgetExists_ShouldSendMessageToQueueAndReturnSuccess()
    {
        // Arrange
        var budget = Budget.Register(_userId, Guid.NewGuid());
        _budgetRepoMock.Setup(r => r.GetByIdAsync(_budgetId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(budget);

        var command = new GenerateBudgetPdfCommand(_budgetId);
        var handler = CreateHandler();

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsError.ShouldBeFalse();
        result.Value.ShouldBe(Result.Success);

        _queueServiceMock.Verify(q => q.SendMessageAsync("budget-reports", It.IsAny<object>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenBudgetDoesNotExist_ShouldReturnNotFoundError()
    {
        // Arrange
        _budgetRepoMock.Setup(r => r.GetByIdAsync(_budgetId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Budget?)null);

        var command = new GenerateBudgetPdfCommand(_budgetId);
        var handler = CreateHandler();

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsError.ShouldBeTrue();
        result.FirstError.Type.ShouldBe(ErrorType.NotFound);
        result.FirstError.Description.ShouldBe(ResourceErrorMessages.TITULO_NAO_ENCONTRADO);

        _queueServiceMock.Verify(q => q.SendMessageAsync(It.IsAny<string>(), It.IsAny<object>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}
