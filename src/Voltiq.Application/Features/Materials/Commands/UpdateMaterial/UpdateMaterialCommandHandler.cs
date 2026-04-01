using ErrorOr;
using MediatR;
using Voltiq.Domain.Interfaces;
using Voltiq.Domain.Interfaces.Repositories.Material;
using Voltiq.Exceptions.Resources;

namespace Voltiq.Application.Features.Materials.Commands.UpdateMaterial;

public sealed class UpdateMaterialCommandHandler(
    IMaterialUpdateOnlyRepository materialUpdateOnlyRepository,
    IUnitOfWork unitOfWork)
    : IRequestHandler<UpdateMaterialCommand, ErrorOr<Updated>>
{
    public async Task<ErrorOr<Updated>> Handle(UpdateMaterialCommand request,
        CancellationToken cancellationToken)
    {
        var material = await materialUpdateOnlyRepository.GetByIdAndUserIdAsync(
            request.Id, request.UserId, cancellationToken);

        if (material is null)
            return Error.NotFound(description: ResourceErrorMessages.MATERIAL_NAO_ENCONTRADO);

        material.Update(request.Name, request.DefaultPrice, request.Unit);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Updated;
    }
}
