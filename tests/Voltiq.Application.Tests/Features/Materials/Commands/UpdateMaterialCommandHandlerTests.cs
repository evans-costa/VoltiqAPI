using ErrorOr;
using Moq;
using Shouldly;
using Voltiq.Application.Features.Materials.Commands.UpdateMaterial;
using Voltiq.Domain.Entities;
using Voltiq.Domain.Enums;
using Voltiq.Domain.Interfaces;
using Voltiq.Domain.Interfaces.Repositories.Material;
using Voltiq.Exceptions.Resources;

namespace Voltiq.Application.Tests.Features.Materials.Commands;

public class UpdateMaterialCommandHandlerTests
{
    private readonly Mock<IMaterialUpdateOnlyRepository> _materialUpdateRepoMock = new();
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();

    private readonly Guid _userId = Guid.NewGuid();

    private UpdateMaterialCommandHandler CreateHandler()
    {
        return new UpdateMaterialCommandHandler(_materialUpdateRepoMock.Object,
            _unitOfWorkMock.Object);
    }

    private static Material MakeMaterial(Guid userId)
    {
        return Material.Register(userId, "Cabo 10mm", 15.50m, MaterialUnit.Metro);
    }

    [Fact]
    public async Task Handle_WithValidCommand_ShouldUpdateMaterialAndReturnUpdated()
    {
        var material = MakeMaterial(_userId);
        _materialUpdateRepoMock
            .Setup(r => r.GetTrackedByIdAndUserIdAsync(material.Id, _userId, It
                .IsAny<CancellationToken>()))
            .ReturnsAsync(material);

        var command = new UpdateMaterialCommand(material.Id, "Fio 6mm", 8.00m, MaterialUnit.Unidade)
            { UserId = _userId };

        var handler = CreateHandler();
        var result = await handler.Handle(command, CancellationToken.None);

        result.IsError.ShouldBeFalse();
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenMaterialNotFound_ShouldReturnNotFoundError()
    {
        _materialUpdateRepoMock
            .Setup(r => r.GetTrackedByIdAndUserIdAsync(It.IsAny<Guid>(), _userId, It
                .IsAny<CancellationToken>()))
            .ReturnsAsync((Material?)null);

        var command =
            new UpdateMaterialCommand(Guid.NewGuid(), "Fio 6mm", 8.00m, MaterialUnit.Unidade)
                { UserId = _userId };

        var handler = CreateHandler();
        var result = await handler.Handle(command, CancellationToken.None);

        result.IsError.ShouldBeTrue();
        result.FirstError.Type.ShouldBe(ErrorType.NotFound);
        result.FirstError.Description.ShouldBe(ResourceErrorMessages.MATERIAL_NAO_ENCONTRADO);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
}
