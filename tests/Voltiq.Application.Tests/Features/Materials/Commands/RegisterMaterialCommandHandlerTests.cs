using ErrorOr;
using Moq;
using Shouldly;
using Voltiq.Application.Features.Materials.Commands.RegisterMaterial;
using Voltiq.Domain.Entities;
using Voltiq.Domain.Enums;
using Voltiq.Domain.Interfaces;
using Voltiq.Domain.Interfaces.Repositories.Material;

namespace Voltiq.Application.Tests.Features.Materials.Commands;

public class RegisterMaterialCommandHandlerTests
{
    private readonly Mock<IMaterialWriteOnlyRepository> _materialWriteRepoMock = new();
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();

    private readonly Guid _userId = Guid.NewGuid();

    private RegisterMaterialCommandHandler CreateHandler() =>
        new(_materialWriteRepoMock.Object, _unitOfWorkMock.Object);

    private RegisterMaterialCommand ValidCommand() =>
        new("Cabo 10mm", 15.50m, MaterialUnit.Metro) { UserId = _userId };

    [Fact]
    public async Task Handle_WithValidCommand_ShouldRegisterMaterialAndReturnResponse()
    {
        var handler = CreateHandler();
        var result = await handler.Handle(ValidCommand(), CancellationToken.None);

        result.IsError.ShouldBeFalse();
        result.Value.Id.ShouldNotBe(Guid.Empty);
        result.Value.Name.ShouldBe("Cabo 10mm");
        result.Value.DefaultPrice.ShouldBe(15.50m);
        result.Value.Unit.ShouldBe(MaterialUnit.Metro);
        result.Value.IsActive.ShouldBeTrue();
        _materialWriteRepoMock.Verify(r => r.AddAsync(It.IsAny<Material>(), It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
