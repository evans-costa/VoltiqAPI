using ErrorOr;
using Moq;
using Shouldly;
using Voltiq.Application.Features.Budgets.Commands.UpdateBudget;
using Voltiq.Domain.Entities;
using Voltiq.Domain.Enums;
using Voltiq.Domain.Interfaces;
using Voltiq.Domain.Interfaces.Repositories.Budget;
using Voltiq.Domain.Interfaces.Repositories.Client;
using Voltiq.Domain.Interfaces.Repositories.Material;
using Voltiq.Domain.ValueObjects;
using Voltiq.Exceptions.Resources;

namespace Voltiq.Application.Tests.Features.Budgets.Commands;

public class UpdateBudgetCommandHandlerTests
{
    private readonly Mock<IBudgetUpdateOnlyRepository> _budgetUpdateRepoMock = new();
    private readonly Mock<IClientReadOnlyRepository> _clientReadRepoMock = new();
    private readonly Mock<IMaterialReadOnlyRepository> _materialReadRepoMock = new();
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();

    private readonly Guid _userId = Guid.NewGuid();
    private readonly Guid _budgetId = Guid.NewGuid();
    private readonly Guid _clientId = Guid.NewGuid();

    private UpdateBudgetCommandHandler CreateHandler()
    {
        return new UpdateBudgetCommandHandler(
            _clientReadRepoMock.Object,
            _materialReadRepoMock.Object,
            _budgetUpdateRepoMock.Object,
            _unitOfWorkMock.Object);
    }

    private Client MakeClient()
    {
        return Client.Register(_userId, "João Silva", "(11) 99999-9999",
            Email.Create("joao@example.com").Value,
            Address.Create("Rua das Flores", "123", "São Paulo", "SP", "01310-100"));
    }

    [Fact]
    public async Task Handle_WithValidData_ShouldUpdateBudgetAndSave()
    {
        // Arrange
        var budget = Budget.Register(_userId, _clientId);
        var oldItem = BudgetItem.Create(budget.Id, null, BudgetItemType.MaoDeObra, null, 2, 15.50m, "Cabo 10mm");
        budget.AddItem(oldItem);

        var client = MakeClient();
        var newClientId = Guid.NewGuid();

        _budgetUpdateRepoMock
            .Setup(r => r.GetTrackedByIdWithItemsAndUserIdAsync(_budgetId, _userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(budget);

        _clientReadRepoMock
            .Setup(r => r.GetByIdAndUserIdAsync(newClientId, _userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(client);

        var command = new UpdateBudgetCommand(
            _budgetId,
            newClientId,
            [
                new UpdateBudgetItemCommand(null, "Disjuntor", BudgetItemType.MaoDeObra, null, 1, 50m)
            ])
        { UserId = _userId };

        var handler = CreateHandler();

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsError.ShouldBeFalse();
        result.Value.ShouldBe(Result.Updated);

        budget.ClientId.ShouldBe(newClientId);
        budget.TotalAmount.ShouldBe(50m);
        budget.Items.Count.ShouldBe(1);
        budget.Items.First().MaterialName.ShouldBe("Disjuntor");

        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenBudgetNotFound_ShouldReturnNotFound()
    {
        // Arrange
        _budgetUpdateRepoMock
            .Setup(r => r.GetTrackedByIdWithItemsAndUserIdAsync(_budgetId, _userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Budget?)null);

        var command = new UpdateBudgetCommand(_budgetId, _clientId, []) { UserId = _userId };
        var handler = CreateHandler();

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsError.ShouldBeTrue();
        result.FirstError.Type.ShouldBe(ErrorType.NotFound);
        result.FirstError.Description.ShouldBe(ResourceErrorMessages.ORCAMENTO_NAO_ENCONTRADO);
    }

    [Fact]
    public async Task Handle_WhenClientNotFound_ShouldReturnNotFound()
    {
        // Arrange
        var budget = Budget.Register(_userId, _clientId);

        _budgetUpdateRepoMock
            .Setup(r => r.GetTrackedByIdWithItemsAndUserIdAsync(_budgetId, _userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(budget);

        _clientReadRepoMock
            .Setup(r => r.GetByIdAndUserIdAsync(_clientId, _userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Client?)null);

        var command = new UpdateBudgetCommand(_budgetId, _clientId, []) { UserId = _userId };
        var handler = CreateHandler();

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsError.ShouldBeTrue();
        result.FirstError.Type.ShouldBe(ErrorType.NotFound);
        result.FirstError.Description.ShouldBe(ResourceErrorMessages.CLIENTE_NAO_ENCONTRADO);
    }

    [Fact]
    public async Task Handle_WhenMaterialNotFound_ShouldReturnNotFound()
    {
        // Arrange
        var budget = Budget.Register(_userId, _clientId);
        var client = MakeClient();
        var materialId = Guid.NewGuid();

        _budgetUpdateRepoMock
            .Setup(r => r.GetTrackedByIdWithItemsAndUserIdAsync(_budgetId, _userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(budget);

        _clientReadRepoMock
            .Setup(r => r.GetByIdAndUserIdAsync(_clientId, _userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(client);

        _materialReadRepoMock
            .Setup(r => r.GetByIdAndUserIdAsync(materialId, _userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Material?)null);

        var command = new UpdateBudgetCommand(
            _budgetId,
            _clientId,
            [
                new UpdateBudgetItemCommand(materialId, "Cabo 10mm", BudgetItemType.Material, MaterialUnit.Metro, 1, 10m)
            ])
        { UserId = _userId };

        var handler = CreateHandler();

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsError.ShouldBeTrue();
        result.FirstError.Type.ShouldBe(ErrorType.NotFound);
        result.FirstError.Description.ShouldBe(ResourceErrorMessages.MATERIAL_NAO_ENCONTRADO);
    }
}
