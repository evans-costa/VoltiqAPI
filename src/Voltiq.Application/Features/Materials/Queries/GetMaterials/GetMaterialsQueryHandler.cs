using ErrorOr;
using MediatR;
using Voltiq.Application.Mappings.Materials;
using Voltiq.Domain.Interfaces.Repositories.Material;

namespace Voltiq.Application.Features.Materials.Queries.GetMaterials;

public sealed class GetMaterialsQueryHandler(IMaterialReadOnlyRepository materialReadOnlyRepository)
    : IRequestHandler<GetMaterialsQuery, ErrorOr<IReadOnlyList<MaterialResponse>>>
{
    public async Task<ErrorOr<IReadOnlyList<MaterialResponse>>> Handle(GetMaterialsQuery request,
        CancellationToken cancellationToken)
    {
        var materials = await materialReadOnlyRepository.GetByUserIdAsync(request.UserId, cancellationToken);

        return materials.Select(m => m.ToResponse()).ToList();
    }
}
