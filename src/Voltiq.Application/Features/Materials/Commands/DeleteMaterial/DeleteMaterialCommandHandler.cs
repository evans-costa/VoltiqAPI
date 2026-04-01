using ErrorOr;
using MediatR;
using Voltiq.Domain.Interfaces;
using Voltiq.Domain.Interfaces.Repositories.Material;
using Voltiq.Exceptions.Resources;

namespace Voltiq.Application.Features.Materials.Commands.DeleteMaterial;

public sealed class DeleteMaterialCommandHandler(
    IMaterialUpdateOnlyRepository materialUpdateOnlyRepository,
    IUnitOfWork unitOfWork)
    : IRequestHandler<DeleteMaterialCommand, ErrorOr<Deleted>>
{
    public async Task<ErrorOr<Deleted>> Handle(DeleteMaterialCommand request,
        CancellationToken cancellationToken)
    {
        var material = await materialUpdateOnlyRepository.GetByIdAndUserIdAsync(
            request.Id, request.UserId, cancellationToken);

        if (material is null)
            return Error.NotFound(description: ResourceErrorMessages.MATERIAL_NAO_ENCONTRADO);

        materialUpdateOnlyRepository.Remove(material);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Deleted;
    }
}
