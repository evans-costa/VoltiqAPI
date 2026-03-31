using ErrorOr;
using MediatR;
using Voltiq.Application.Mappings.Materials;
using Voltiq.Domain.Entities;
using Voltiq.Domain.Interfaces;
using Voltiq.Domain.Interfaces.Repositories.Material;

namespace Voltiq.Application.Features.Materials.Commands.RegisterMaterial;

public sealed class RegisterMaterialCommandHandler(
    IMaterialWriteOnlyRepository materialWriteOnlyRepository,
    IUnitOfWork unitOfWork)
    : IRequestHandler<RegisterMaterialCommand, ErrorOr<MaterialResponse>>
{
    public async Task<ErrorOr<MaterialResponse>> Handle(RegisterMaterialCommand request,
        CancellationToken cancellationToken)
    {
        var material = Material.Register(request.UserId, request.Name, request.DefaultPrice, request.Unit);

        await materialWriteOnlyRepository.AddAsync(material, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return material.ToResponse();
    }
}
