using ErrorOr;
using Moq;
using Shouldly;
using Voltiq.Application.Common.Interfaces.Storage;
using Voltiq.Application.Features.Budgets.Queries.GetBudgetPdfUrl;
using Voltiq.Domain.Entities;
using Voltiq.Domain.Interfaces.Repositories.Budget;
using Voltiq.Exceptions.Resources;

namespace Voltiq.Application.Tests.Features.Budgets.Queries;

public class GetBudgetPdfUrlQueryHandlerTests
{
    private readonly Mock<IBudgetReadOnlyRepository> _budgetRepoMock = new();
    private readonly Mock<IStorageService> _storageServiceMock = new();
    private readonly Guid _budgetId = Guid.NewGuid();
    private readonly Guid _userId = Guid.NewGuid();

    private GetBudgetPdfUrlQueryHandler CreateHandler()
    {
        return new GetBudgetPdfUrlQueryHandler(_storageServiceMock.Object, _budgetRepoMock.Object);
    }

    [Fact]
    public async Task Handle_WhenPdfExists_ShouldReturnUrl()
    {
        // Arrange
        var budget = Budget.Register(_userId, Guid.NewGuid());
        _budgetRepoMock.Setup(r => r.GetByIdAsync(_budgetId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(budget);
            
        var expectedUrl = "https://azure.blob/reports/budget.pdf?sas=token";
        _storageServiceMock.Setup(s => s.GetSasUrlAsync($"budget-{_budgetId}.pdf", 1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedUrl);

        var query = new GetBudgetPdfUrlQuery(_budgetId);
        var handler = CreateHandler();

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsError.ShouldBeFalse();
        result.Value.ShouldBe(expectedUrl);
    }

    [Fact]
    public async Task Handle_WhenPdfDoesNotExistInStorage_ShouldReturnNotFoundError()
    {
        // Arrange
        var budget = Budget.Register(_userId, Guid.NewGuid());
        _budgetRepoMock.Setup(r => r.GetByIdAsync(_budgetId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(budget);
            
        _storageServiceMock.Setup(s => s.GetSasUrlAsync($"budget-{_budgetId}.pdf", 1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(string.Empty);

        var query = new GetBudgetPdfUrlQuery(_budgetId);
        var handler = CreateHandler();

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsError.ShouldBeTrue();
        result.FirstError.Type.ShouldBe(ErrorType.NotFound);
        result.FirstError.Code.ShouldBe("Budget.PdfNotGenerated");
    }

    [Fact]
    public async Task Handle_WhenBudgetDoesNotExist_ShouldReturnNotFoundError()
    {
        // Arrange
        _budgetRepoMock.Setup(r => r.GetByIdAsync(_budgetId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Budget?)null);

        var query = new GetBudgetPdfUrlQuery(_budgetId);
        var handler = CreateHandler();

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsError.ShouldBeTrue();
        result.FirstError.Type.ShouldBe(ErrorType.NotFound);
        result.FirstError.Description.ShouldBe(ResourceErrorMessages.TITULO_NAO_ENCONTRADO);

        _storageServiceMock.Verify(s => s.GetSasUrlAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}
