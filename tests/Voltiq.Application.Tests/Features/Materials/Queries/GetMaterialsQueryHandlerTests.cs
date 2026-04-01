using Moq;
using Shouldly;
using Voltiq.Application.Features.Materials.Queries.GetMaterials;
using Voltiq.Domain.Entities;
using Voltiq.Domain.Enums;
using Voltiq.Domain.Interfaces.Repositories.Material;

namespace Voltiq.Application.Tests.Features.Materials.Queries;

public class GetMaterialsQueryHandlerTests
{
    private readonly Mock<IMaterialReadOnlyRepository> _materialReadRepoMock = new();

    private readonly Guid _userId = Guid.NewGuid();

    private GetMaterialsQueryHandler CreateHandler() =>
        new(_materialReadRepoMock.Object);

    [Fact]
    public async Task Handle_ShouldReturnAllMaterialsForUser()
    {
        var materials = new List<Material>
        {
            Material.Register(_userId, "Cabo 10mm", 15.50m, MaterialUnit.Metro),
            Material.Register(_userId, "Fio 6mm", 8.00m, MaterialUnit.Unidade),
        };

        _materialReadRepoMock
            .Setup(r => r.GetByUserIdAsync(_userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(materials);

        var query = new GetMaterialsQuery { UserId = _userId };

        var handler = CreateHandler();
        var result = await handler.Handle(query, CancellationToken.None);

        result.IsError.ShouldBeFalse();
        result.Value.Count.ShouldBe(2);
        result.Value[0].Name.ShouldBe("Cabo 10mm");
        result.Value[1].Name.ShouldBe("Fio 6mm");
    }

    [Fact]
    public async Task Handle_WhenNoMaterials_ShouldReturnEmptyList()
    {
        _materialReadRepoMock
            .Setup(r => r.GetByUserIdAsync(_userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Material>());

        var query = new GetMaterialsQuery { UserId = _userId };

        var handler = CreateHandler();
        var result = await handler.Handle(query, CancellationToken.None);

        result.IsError.ShouldBeFalse();
        result.Value.ShouldBeEmpty();
    }
}
