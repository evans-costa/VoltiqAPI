using ErrorOr;
using MediatR;
using Voltiq.Application.Mappings.Materials;
using Voltiq.Domain.Interfaces.Repositories.Material;
using Voltiq.Exceptions.Resources;

namespace Voltiq.Application.Features.Materials.Queries.GetMaterialById;

public sealed class GetMaterialByIdQueryHandler(IMaterialReadOnlyRepository materialReadOnlyRepository)
    : IRequestHandler<GetMaterialByIdQuery, ErrorOr<MaterialResponse>>
{
    public async Task<ErrorOr<MaterialResponse>> Handle(GetMaterialByIdQuery request,
        CancellationToken cancellationToken)
    {
        var material = await materialReadOnlyRepository.GetByIdAndUserIdAsync(
            request.Id, request.UserId, cancellationToken);

        if (material is null)
            return Error.NotFound(description: ResourceErrorMessages.MATERIAL_NAO_ENCONTRADO);

        return material.ToResponse();
    }
}
