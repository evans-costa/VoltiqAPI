using ErrorOr;
using Moq;
using Shouldly;
using Voltiq.Application.Features.Materials.Queries.GetMaterialById;
using Voltiq.Domain.Entities;
using Voltiq.Domain.Enums;
using Voltiq.Domain.Interfaces.Repositories.Material;
using Voltiq.Exceptions.Resources;

namespace Voltiq.Application.Tests.Features.Materials.Queries;

public class GetMaterialByIdQueryHandlerTests
{
    private readonly Mock<IMaterialReadOnlyRepository> _materialReadRepoMock = new();

    private readonly Guid _userId = Guid.NewGuid();

    private GetMaterialByIdQueryHandler CreateHandler() =>
        new(_materialReadRepoMock.Object);

    [Fact]
    public async Task Handle_WhenMaterialExists_ShouldReturnResponse()
    {
        var material = Material.Register(_userId, "Cabo 10mm", 15.50m, MaterialUnit.Metro);

        _materialReadRepoMock
            .Setup(r => r.GetByIdAndUserIdAsync(material.Id, _userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(material);

        var query = new GetMaterialByIdQuery(material.Id) { UserId = _userId };

        var handler = CreateHandler();
        var result = await handler.Handle(query, CancellationToken.None);

        result.IsError.ShouldBeFalse();
        result.Value.Id.ShouldBe(material.Id);
        result.Value.Name.ShouldBe("Cabo 10mm");
        result.Value.DefaultPrice.ShouldBe(15.50m);
        result.Value.Unit.ShouldBe(MaterialUnit.Metro);
        result.Value.IsActive.ShouldBeTrue();
    }

    [Fact]
    public async Task Handle_WhenMaterialNotFound_ShouldReturnNotFoundError()
    {
        _materialReadRepoMock
            .Setup(r => r.GetByIdAndUserIdAsync(It.IsAny<Guid>(), _userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Material?)null);

        var query = new GetMaterialByIdQuery(Guid.NewGuid()) { UserId = _userId };

        var handler = CreateHandler();
        var result = await handler.Handle(query, CancellationToken.None);

        result.IsError.ShouldBeTrue();
        result.FirstError.Type.ShouldBe(ErrorType.NotFound);
        result.FirstError.Description.ShouldBe(ResourceErrorMessages.MATERIAL_NAO_ENCONTRADO);
    }
}
