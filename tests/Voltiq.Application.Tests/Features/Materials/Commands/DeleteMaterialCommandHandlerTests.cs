using ErrorOr;
using Moq;
using Shouldly;
using Voltiq.Application.Features.Materials.Commands.DeleteMaterial;
using Voltiq.Domain.Entities;
using Voltiq.Domain.Enums;
using Voltiq.Domain.Interfaces;
using Voltiq.Domain.Interfaces.Repositories.Material;
using Voltiq.Exceptions.Resources;

namespace Voltiq.Application.Tests.Features.Materials.Commands;

public class DeleteMaterialCommandHandlerTests
{
    private readonly Mock<IMaterialUpdateOnlyRepository> _materialUpdateRepoMock = new();
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();

    private readonly Guid _userId = Guid.NewGuid();

    private DeleteMaterialCommandHandler CreateHandler()
    {
        return new DeleteMaterialCommandHandler(_materialUpdateRepoMock.Object,
            _unitOfWorkMock.Object);
    }

    private static Material MakeMaterial(Guid userId)
    {
        return Material.Register(userId, "Cabo 10mm", 15.50m, MaterialUnit.Metro);
    }

    [Fact]
    public async Task Handle_WhenMaterialExists_ShouldDeleteAndReturnDeleted()
    {
        var material = MakeMaterial(_userId);
        _materialUpdateRepoMock
            .Setup(r => r.GetTrackedByIdAndUserIdAsync(material.Id, _userId, It
                .IsAny<CancellationToken>()))
            .ReturnsAsync(material);

        var command = new DeleteMaterialCommand(material.Id) { UserId = _userId };

        var handler = CreateHandler();
        var result = await handler.Handle(command, CancellationToken.None);

        result.IsError.ShouldBeFalse();
        _materialUpdateRepoMock.Verify(r => r.Remove(material), Times.Once);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenMaterialNotFound_ShouldReturnNotFoundError()
    {
        _materialUpdateRepoMock
            .Setup(r => r.GetTrackedByIdAndUserIdAsync(It.IsAny<Guid>(), _userId, It
                .IsAny<CancellationToken>()))
            .ReturnsAsync((Material?)null);

        var command = new DeleteMaterialCommand(Guid.NewGuid()) { UserId = _userId };

        var handler = CreateHandler();
        var result = await handler.Handle(command, CancellationToken.None);

        result.IsError.ShouldBeTrue();
        result.FirstError.Type.ShouldBe(ErrorType.NotFound);
        result.FirstError.Description.ShouldBe(ResourceErrorMessages.MATERIAL_NAO_ENCONTRADO);
        _materialUpdateRepoMock.Verify(r => r.Remove(It.IsAny<Material>()), Times.Never);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
}
