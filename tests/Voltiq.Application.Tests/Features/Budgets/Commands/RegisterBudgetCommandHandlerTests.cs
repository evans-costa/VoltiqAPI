using ErrorOr;
using Moq;
using Shouldly;
using Voltiq.Application.Features.Budgets.Commands.RegisterBudget;
using Voltiq.Domain.Entities;
using Voltiq.Domain.Enums;
using Voltiq.Domain.Interfaces;
using Voltiq.Domain.Interfaces.Repositories.Budget;
using Voltiq.Domain.Interfaces.Repositories.Client;
using Voltiq.Domain.Interfaces.Repositories.Material;
using Voltiq.Domain.ValueObjects;
using Voltiq.Exceptions.Resources;

namespace Voltiq.Application.Tests.Features.Budgets.Commands;

public class RegisterBudgetCommandHandlerTests
{
    private readonly Mock<IBudgetWriteOnlyRepository> _budgetWriteRepoMock = new();
    private readonly Guid _clientId = Guid.NewGuid();
    private readonly Mock<IClientReadOnlyRepository> _clientReadRepoMock = new();
    private readonly Mock<IMaterialReadOnlyRepository> _materialReadRepoMock = new();
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();

    private readonly Guid _userId = Guid.NewGuid();

    private RegisterBudgetCommandHandler CreateHandler()
    {
        return new RegisterBudgetCommandHandler(_clientReadRepoMock.Object,
            _materialReadRepoMock.Object,
            _budgetWriteRepoMock.Object, _unitOfWorkMock.Object);
    }

    private Client MakeClient()
    {
        return Client.Register(_userId, "João Silva", "(11) 99999-9999",
            Email.Create("joao@example.com").Value,
            Address.Create("Rua das Flores", "123", "São Paulo", "SP", "01310-100"));
    }

    private RegisterBudgetCommand CommandWithCustomItem()
    {
        return new RegisterBudgetCommand(_clientId, [
                new RegisterBudgetItemCommand(null, "Cabo 10mm", BudgetItemType.MaoDeObra, null,
                    2, 15.50m)
            ])
        { UserId = _userId };
    }

    [Fact]
    public async Task Handle_WithCustomItems_ShouldRegisterBudgetAndReturnDetailResponse()
    {
        var client = MakeClient();
        _clientReadRepoMock
            .Setup(r => r.GetByIdAndUserIdAsync(_clientId, _userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(client);

        var handler = CreateHandler();
        var result = await handler.Handle(CommandWithCustomItem(), CancellationToken.None);

        result.IsError.ShouldBeFalse();
        result.Value.Id.ShouldNotBe(Guid.Empty);
        result.Value.Status.ShouldBe(BudgetStatus.Draft);
        result.Value.TotalAmount.ShouldBe(31.00m);
        result.Value.Client.Name.ShouldBe("João Silva");
        result.Value.Items.Count.ShouldBe(1);
        result.Value.Items[0].MaterialName.ShouldBe("Cabo 10mm");
        result.Value.Items[0].TotalPrice.ShouldBe(31.00m);

        _budgetWriteRepoMock.Verify(
            r => r.AddAsync(It.IsAny<Budget>(), It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WithMaterialId_ShouldValidateMaterialAndRegisterBudget()
    {
        var materialId = Guid.NewGuid();
        var client = MakeClient();
        var material = Material.Register(_userId, "Cabo 10mm", 15.50m, MaterialUnit.Metro);

        _clientReadRepoMock
            .Setup(r => r.GetByIdAndUserIdAsync(_clientId, _userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(client);
        _materialReadRepoMock
            .Setup(r => r.GetByIdAndUserIdAsync(materialId, _userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(material);

        var command = new RegisterBudgetCommand(
                _clientId,
                [
                    new RegisterBudgetItemCommand(materialId, "Cabo 10mm", BudgetItemType.Material,
                        MaterialUnit.Metro, 3, 10.00m)
                ])
        { UserId = _userId };

        var handler = CreateHandler();
        var result = await handler.Handle(command, CancellationToken.None);

        result.IsError.ShouldBeFalse();
        result.Value.Items[0].MaterialId.ShouldBe(materialId);
        _materialReadRepoMock.Verify(
            r => r.GetByIdAndUserIdAsync(materialId, _userId, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_WhenClientNotFound_ShouldReturnNotFoundError()
    {
        _clientReadRepoMock
            .Setup(r =>
                r.GetByIdAndUserIdAsync(It.IsAny<Guid>(), _userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Client?)null);

        var handler = CreateHandler();
        var result = await handler.Handle(CommandWithCustomItem(), CancellationToken.None);

        result.IsError.ShouldBeTrue();
        result.FirstError.Type.ShouldBe(ErrorType.NotFound);
        result.FirstError.Description.ShouldBe(ResourceErrorMessages.CLIENTE_NAO_ENCONTRADO);
        _budgetWriteRepoMock.Verify(
            r => r.AddAsync(It.IsAny<Budget>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WhenMaterialIdNotFound_ShouldReturnNotFoundError()
    {
        var client = MakeClient();
        _clientReadRepoMock
            .Setup(r => r.GetByIdAndUserIdAsync(_clientId, _userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(client);
        _materialReadRepoMock
            .Setup(r =>
                r.GetByIdAndUserIdAsync(It.IsAny<Guid>(), _userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Material?)null);

        var command = new RegisterBudgetCommand(
                _clientId,
                [
                    new RegisterBudgetItemCommand(Guid.NewGuid(), "Cabo 10mm",
                        BudgetItemType.Material, MaterialUnit.Metro, 1, 10.00m)
                ])
        { UserId = _userId };

        var handler = CreateHandler();
        var result = await handler.Handle(command, CancellationToken.None);

        result.IsError.ShouldBeTrue();
        result.FirstError.Type.ShouldBe(ErrorType.NotFound);
        result.FirstError.Description.ShouldBe(ResourceErrorMessages.MATERIAL_NAO_ENCONTRADO);
        _budgetWriteRepoMock.Verify(
            r => r.AddAsync(It.IsAny<Budget>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}
